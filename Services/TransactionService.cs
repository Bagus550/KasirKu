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
            int? shiftId = null);
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
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionResult> ProcessTransactionAsync(
            List<CartItem> cartItems,
            decimal totalBayar,
            int kasirId,
            int? shiftId = null)
        {
            // 1. Validasi Keranjang
            if (cartItems == null || !cartItems.Any())
            {
                return new TransactionResult { IsSuccess = false, Message = "Keranjang belanja kosong." };
            }

            // 2. Hitung Total Belanja
            decimal totalHarga = cartItems.Sum(item => item.Subtotal);

            // 3. Validasi Pembayaran
            if (totalBayar < totalHarga)
            {
                return new TransactionResult
                {
                    IsSuccess = false,
                    Message = $"Uang pembayaran kurang! Total: Rp{totalHarga:N0}, Dibayar: Rp{totalBayar:N0}"
                };
            }

            decimal kembalian = totalBayar - totalHarga;

            // Gunakan Transaction Scope EF Core agar jika simpan gagal, stok tidak ikut berkurang (ACID)
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var detailTransaksiList = new List<DetailTransaksi>();

                // 4. Pengurangan Stok Produk & Mapping CartItem ke DetailTransaksi
                foreach (var cartItem in cartItems)
                {
                    var produk = await _context.Produk.FindAsync(cartItem.Produk.Id);

                    if (produk == null)
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
                            Message = $"Stok '{produk.Nama}' tidak mencukupi. Sisa stok: {produk.Stok}"
                        };
                    }

                    // Potong Stok Produk (Model Produk harus memiliki atribut [ConcurrencyCheck] di property Stok)
                    produk.Stok -= cartItem.Jumlah;

                    // Konversi CartItem ke DetailTransaksi
                    detailTransaksiList.Add(new DetailTransaksi
                    {
                        ProdukId = cartItem.Produk.Id,
                        NamaProduk = cartItem.Produk.Nama,
                        HargaJual = cartItem.HargaJual,
                        Jumlah = cartItem.Jumlah
                    });
                }

                // 5. Buat Header Transaksi
                var transaksi = new Transaksi
                {
                    NomorNota = $"INV/{DateTime.Now:yyyyMMdd}/{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    Tanggal = DateTime.Now,
                    TotalHarga = totalHarga,
                    TotalBayar = totalBayar,
                    Kembalian = kembalian,
                    KasirSessionId = shiftId,
                    NamaKasir = SessionManager.CurrentKasir?.Nama ?? "Admin",
                    DetailTransaksi = detailTransaksiList
                };

                // Update juga total omzet tunai di Sesi Kasir jika ada sesi aktif
                if (shiftId.HasValue)
                {
                    var currentSessionDb = await _context.KasirSession.FindAsync(shiftId.Value);
                    if (currentSessionDb != null)
                    {
                        currentSessionDb.TotalTunaiSistem += totalHarga;
                    }
                }

                _context.Transaksi.Add(transaksi);

                // 6. Catat Audit Log
                var auditLog = new AuditLog
                {
                    KasirId = kasirId,
                    Waktu = DateTime.Now,
                    JenisAksi = "PROSES_TRANSAKSI",
                    Keterangan = $"Berhasil memproses transaksi {transaksi.NomorNota} senilai Rp{totalHarga:N0}"
                };
                _context.AuditLog.Add(auditLog);

                // 7. Commit ke Database SQLite
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new TransactionResult
                {
                    IsSuccess = true,
                    Message = "Transaksi berhasil disimpan!",
                    Kembalian = kembalian,
                    TransaksiData = transaksi
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                // Rollback jika ada konflik update stok bersamaan
                await dbTransaction.RollbackAsync();

                return new TransactionResult
                {
                    IsSuccess = false,
                    Message = "Gagal memproses transaksi: Stok barang telah berubah atau diubah oleh transaksi/kasir lain. Silakan coba lagi."
                };
            }
            catch (Exception ex)
            {
                // Rollback jika ada error tak terduga lainnya
                await dbTransaction.RollbackAsync();

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