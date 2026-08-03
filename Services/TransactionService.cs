using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KasirKu.Data;
using KasirKu.Models;

namespace KasirKu.Services
{
    public interface ITransactionService
    {
        Task<TransactionResult> ProcessTransactionAsync(
            List<CartItem> cartItems,
            decimal totalBayar,
            int kasirId,
            int? kasirSessionId = null);
    }

    public class TransactionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal Kembalian { get; set; }
        public Transaksi? TransaksiData { get; set; }
    }

    public class TransactionService : ITransactionService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILoggerService? _logger;

        public TransactionService(IDbContextFactory<AppDbContext> contextFactory, ILoggerService? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<TransactionResult> ProcessTransactionAsync(
            List<CartItem> cartItems,
            decimal totalBayar,
            int kasirId,
            int? kasirSessionId = null)
        {
            // 1. Validasi Input
            if (cartItems == null || !cartItems.Any())
            {
                return new TransactionResult { IsSuccess = false, Message = "Keranjang belanja kosong." };
            }

            decimal totalHarga = cartItems.Sum(item => item.Subtotal);

            if (totalBayar < totalHarga)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    Message = $"Uang pembayaran kurang! Total: Rp{totalHarga:N0}, Dibayar: Rp{totalBayar:N0}"
                };
            }

            decimal kembalian = totalBayar - totalHarga;

            // 2. Eksekusi Database Operation dengan Concurrency Control
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                await using var dbTransaction = await context.Database.BeginTransactionAsync();

                var productIds = cartItems.Select(c => c.Produk.Id).ToList();
                var produkDbList = await context.Produk
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                var detailTransaksiList = new List<DetailTransaksi>();

                foreach (var cartItem in cartItems)
                {
                    if (!produkDbList.TryGetValue(cartItem.Produk.Id, out var produk))
                    {
                        await dbTransaction.RollbackAsync();
                        return new TransactionResult
                        {
                            IsSuccess = false,
                            Message = $"Produk '{cartItem.Produk.Nama}' tidak ditemukan di database."
                        };
                    }

                    if (produk.Stok < cartItem.Jumlah)
                    {
                        await dbTransaction.RollbackAsync();
                        return new TransactionResult
                        {
                            IsSuccess = false,
                            Message = $"Stok '{produk.Nama}' tidak mencukupi. Sisa stok saat ini: {produk.Stok}"
                        };
                    }

                    // Potong Stok
                    produk.Stok -= cartItem.Jumlah;

                    detailTransaksiList.Add(new DetailTransaksi
                    {
                        ProdukId = cartItem.Produk.Id,
                        NamaProduk = cartItem.Produk.Nama,
                        HargaJual = cartItem.HargaJual,
                        Jumlah = cartItem.Jumlah
                    });
                }

                // 3. Buat Header Transaksi
                var transaksi = new Transaksi
                {
                    NomorNota = $"INV/{DateTime.Now:yyyyMMdd}/{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    Tanggal = DateTime.Now,
                    TotalHarga = totalHarga,
                    TotalBayar = totalBayar,
                    Kembalian = kembalian,
                    KasirSessionId = kasirSessionId,
                    NamaKasir = SessionManager.CurrentKasir?.Nama ?? "Kasir",
                    DetailTransaksi = detailTransaksiList
                };

                if (kasirSessionId.HasValue)
                {
                    var currentSessionDb = await context.KasirSession.FindAsync(kasirSessionId.Value);
                    if (currentSessionDb != null)
                    {
                        currentSessionDb.TotalTunaiSistem += totalHarga;
                    }
                }

                context.Transaksi.Add(transaksi);

                // 4. Catat Audit Log
                context.AuditLog.Add(new AuditLog
                {
                    KasirId = kasirId,
                    Waktu = DateTime.Now,
                    JenisAksi = "PROSES_TRANSAKSI",
                    Keterangan = $"Berhasil memproses transaksi {transaksi.NomorNota} senilai Rp{totalHarga:N0}"
                });

                // 5. Commit batch perubahan
                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new TransactionResult
                {
                    IsSuccess = true,
                    Message = "Transaksi berhasil disimpan!",
                    Kembalian = kembalian,
                    TransaksiData = transaksi
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Bentrokan konkurensi stok saat transaksi.");
                return new TransactionResult
                {
                    IsSuccess = false,
                    Message = "Gagal memproses transaksi: Stok produk telah diperbarui oleh pengguna/transaksi lain secara bersamaan. Silakan coba lagi."
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Gagal memproses transaksi");
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return new TransactionResult
                {
                    IsSuccess = false,
                    Message = $"Terjadi kesalahan saat menyimpan transaksi: {innerMsg}"
                };
            }
        }
    }
}