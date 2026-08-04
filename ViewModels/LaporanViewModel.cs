using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public class ProdukTerlarisModel
    {
        public string NamaProduk { get; set; } = string.Empty;
        public int TotalTerjual { get; set; }
    }

    public partial class LaporanViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // Filter Tanggal
        [ObservableProperty]
        private DateTime _tanggalAwal = DateTime.Today;

        [ObservableProperty]
        private DateTime _tanggalAkhir = DateTime.Today;
        public ObservableCollection<Transaksi> DaftarTransaksi { get; } = new();
        public ObservableCollection<ProdukTerlarisModel> ProdukTerlaris { get; } = new();
        public ObservableCollection<Produk> StokKritis { get; } = new();

        [ObservableProperty]
        private Transaksi? _transaksiTerpilih;

        [ObservableProperty]
        private decimal _totalOmzet;

        [ObservableProperty]
        private int _totalJumlahTransaksi;

        [ObservableProperty]
        private int _totalItemTerjual;

        public LaporanViewModel(IDialogService dialogService, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dialogService = dialogService;
            _contextFactory = contextFactory;

            _ = MuatLaporanAsync();
        }

        [RelayCommand]
        public async Task MuatLaporanAsync()
        {
            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();

                DateTime tglMulai = TanggalAwal.Date;
                DateTime tglSelesai = TanggalAkhir.Date.AddDays(1).AddTicks(-1);

                var queryBase = db.Transaksi
                    .AsNoTracking()
                    .Where(t => t.Tanggal >= tglMulai && t.Tanggal <= tglSelesai);

                var listTransaksi = await queryBase
                    .Include(t => t.DetailTransaksi)
                    .OrderByDescending(t => t.Tanggal)
                    .ToListAsync();

                DaftarTransaksi.Clear();
                foreach (var item in listTransaksi)
                {
                    DaftarTransaksi.Add(item);
                }

                TotalOmzet = listTransaksi.Sum(t => t.TotalHarga);
                TotalJumlahTransaksi = listTransaksi.Count;
                TotalItemTerjual = listTransaksi.SelectMany(t => t.DetailTransaksi).Sum(d => d.Jumlah);

                TransaksiTerpilih = null;

                await MuatWidgetLaporanAsync(db, tglMulai, tglSelesai);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat laporan: {ex.Message}");
            }
        }

        private async Task MuatWidgetLaporanAsync(AppDbContext db, DateTime tglMulai, DateTime tglSelesai)
        {
            // 1. Top 5 Produk Terlaris
            var listTerlaris = await db.DetailTransaksi
                .AsNoTracking()
                .Where(d => d.Transaksi.Tanggal >= tglMulai && d.Transaksi.Tanggal <= tglSelesai)
                .GroupBy(d => d.NamaProduk)
                .Select(g => new ProdukTerlarisModel
                {
                    NamaProduk = g.Key,
                    TotalTerjual = g.Sum(x => x.Jumlah)
                })
                .OrderByDescending(x => x.TotalTerjual)
                .Take(5)
                .ToListAsync();

            ProdukTerlaris.Clear();
            foreach (var item in listTerlaris)
            {
                ProdukTerlaris.Add(item);
            }

            // 2. Produk Stok Kritis
            var listStokKritis = await db.Produk
                .AsNoTracking()
                .Where(p => p.Stok <= p.StokMinimum)
                .OrderBy(p => p.Stok)
                .ToListAsync();

            StokKritis.Clear();
            foreach (var item in listStokKritis)
            {
                StokKritis.Add(item);
            }
        }

        // Quick Filter: Hari Ini
        [RelayCommand]
        public async Task FilterHariIniAsync()
        {
            TanggalAwal = DateTime.Today;
            TanggalAkhir = DateTime.Today;
            await MuatLaporanAsync();
        }

        // Quick Filter: Bulan Ini
        [RelayCommand]
        public async Task FilterBulanIniAsync()
        {
            var now = DateTime.Now;
            TanggalAwal = new DateTime(now.Year, now.Month, 1);
            TanggalAkhir = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            await MuatLaporanAsync();
        }

        [RelayCommand]
        public void ExportCsv()
        {
            if (DaftarTransaksi.Count == 0)
            {
                _dialogService.ShowWarning("Tidak ada data transaksi untuk diekspor!", "Peringatan");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Laporan_Penjualan_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Nomor Nota;Tanggal;Kasir;Total Belanja;Total Bayar;Kembalian");

                    foreach (var t in DaftarTransaksi)
                    {
                        sb.AppendLine($"{t.NomorNota};{t.Tanggal:dd/MM/yyyy HH:mm};{t.NamaKasir};{t.TotalHarga};{t.TotalBayar};{t.Kembalian}");
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    _dialogService.ShowInfo("Laporan berhasil diekspor ke file CSV!", "Sukses Export");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Gagal mengekspor data: {ex.Message}", "Error Export");
                }
            }
        }

        [RelayCommand]
        public async Task BackupDatabaseAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "SQLite Database (*.sqlite;*.db)|*.sqlite;*.db",
                    FileName = $"kasirku_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string destinationPath = saveFileDialog.FileName;

                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    using var db = await _contextFactory.CreateDbContextAsync();
                    await db.Database.ExecuteSqlRawAsync("VACUUM INTO {0};", destinationPath);

                    _dialogService.ShowInfo("Backup database berhasil disimpan!", "Sukses Backup");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal membuat backup database: {ex.Message}", "Error Backup");
            }
        }
    }
}