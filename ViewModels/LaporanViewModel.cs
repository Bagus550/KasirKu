using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

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

        // Filter Tanggal
        [ObservableProperty]
        private DateTime _tanggalAwal = DateTime.Today;

        [ObservableProperty]
        private DateTime _tanggalAkhir = DateTime.Today;

        // Daftar Transaksi Hasil Filter
        [ObservableProperty]
        private ObservableCollection<Transaksi> _daftarTransaksi = new();

        // Transaksi yang Sedang Dipilih di Tabel (untuk lihat detail item)
        [ObservableProperty]
        private Transaksi? _transaksiTerpilih;

        // Ringkasan Dashboard
        [ObservableProperty]
        private decimal _totalOmzet;

        [ObservableProperty]
        private int _totalJumlahTransaksi;

        [ObservableProperty]
        private int _totalItemTerjual;

        [ObservableProperty]
        private ObservableCollection<ProdukTerlarisModel> _produkTerlaris = new();

        [ObservableProperty]
        private ObservableCollection<Produk> _stokKritis = new();

        // Constructor Utama: Menerima IDialogService dari DI Container
        public LaporanViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            MuatLaporan();
        }

        [RelayCommand]
        public void MuatLaporan()
        {
            try
            {
                using var db = new AppDbContext();

                // Atur rentang waktu dari jam 00:00:00 tanggal awal sampai 23:59:59 tanggal akhir
                DateTime tglMulai = TanggalAwal.Date;
                DateTime tglSelesai = TanggalAkhir.Date.AddDays(1).AddTicks(-1);

                var query = db.Transaksi
                    .Include(t => t.DetailTransaksi)
                    .Where(t => t.Tanggal >= tglMulai && t.Tanggal <= tglSelesai)
                    .OrderByDescending(t => t.Tanggal)
                    .ToList();

                DaftarTransaksi = new ObservableCollection<Transaksi>(query);

                // Hitung Ringkasan Data
                TotalOmzet = query.Sum(t => t.TotalHarga);
                TotalJumlahTransaksi = query.Count;
                TotalItemTerjual = query.SelectMany(t => t.DetailTransaksi).Sum(d => d.Jumlah);

                // Reset transaksi terpilih
                TransaksiTerpilih = null;

                MuatWidgetLaporan(db, tglMulai, tglSelesai);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat laporan: {ex.Message}");
            }
        }

        private void MuatWidgetLaporan(AppDbContext db, DateTime tglMulai, DateTime tglSelesai)
        {
            // 1. Hitung Top 5 Produk Terlaris berdasarkan range tanggal laporan
            var queryTerlaris = db.DetailTransaksi
                .Where(d => d.Transaksi.Tanggal >= tglMulai && d.Transaksi.Tanggal <= tglSelesai)
                .GroupBy(d => d.NamaProduk)
                .Select(g => new ProdukTerlarisModel
                {
                    NamaProduk = g.Key,
                    TotalTerjual = g.Sum(x => x.Jumlah)
                })
                .OrderByDescending(x => x.TotalTerjual)
                .Take(5)
                .ToList();

            ProdukTerlaris = new ObservableCollection<ProdukTerlarisModel>(queryTerlaris);

            // 2. Ambil Produk dengan Stok Kritis (Stok <= StokMinimum)
            var queryStokKritis = db.Produk
                .Where(p => p.Stok <= p.StokMinimum)
                .OrderBy(p => p.Stok)
                .ToList();

            StokKritis = new ObservableCollection<Produk>(queryStokKritis);
        }

        // Quick Filter: Hari Ini
        [RelayCommand]
        public void FilterHariIni()
        {
            TanggalAwal = DateTime.Today;
            TanggalAkhir = DateTime.Today;
            MuatLaporan();
        }

        // Quick Filter: Bulan Ini
        [RelayCommand]
        public void FilterBulanIni()
        {
            var now = DateTime.Now;
            TanggalAwal = new DateTime(now.Year, now.Month, 1);
            TanggalAkhir = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            MuatLaporan();
        }

        [RelayCommand]
        public void ExportCsv()
        {
            if (DaftarTransaksi == null || DaftarTransaksi.Count == 0)
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
                    // Header Kolom CSV
                    sb.AppendLine("Nomor Nota;Tanggal;Kasir;Total Belanja;Total Bayar;Kembalian");

                    // Isi Baris Transaksi
                    foreach (var t in DaftarTransaksi)
                    {
                        sb.AppendLine($"{t.NomorNota};{t.Tanggal:dd/MM/yyyy HH:mm};{t.NamaKasir};{t.TotalHarga};{t.TotalBayar};{t.Kembalian}");
                    }

                    // Tulis ke file menggunakan encoding UTF8 dengan BOM agar tanda pemisah terbaca rapi di Microsoft Excel
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
        public void BackupDatabase()
        {
            try
            {
                string sourceDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");

                if (!File.Exists(sourceDb))
                {
                    _dialogService.ShowError("File database tidak ditemukan!", "Error");
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "SQLite Database (*.sqlite;*.db)|*.sqlite;*.db",
                    FileName = $"kasirku_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Copy file database fisik ke direktori tujuan
                    File.Copy(sourceDb, saveFileDialog.FileName, overwrite: true);

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