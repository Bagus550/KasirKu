using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KasirKu.ViewModels
{
    public partial class LaporanViewModel : ObservableObject
    {
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

        public LaporanViewModel()
        {
            MuatLaporan();
        }

        [RelayCommand]
        public void MuatLaporan()
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
    }
}