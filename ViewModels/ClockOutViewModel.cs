using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class ClockOutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _namaKasir = string.Empty;

        [ObservableProperty]
        private string _namaShift = string.Empty;

        [ObservableProperty]
        private decimal _modalAwal;

        [ObservableProperty]
        private decimal _totalOmzetTunai;

        [ObservableProperty]
        private decimal _totalEkspektasi; // Modal Awal + Total Penjualan

        [ObservableProperty]
        private string _totalAktifFisikText = "0";

        [ObservableProperty]
        private decimal _selisih;

        [ObservableProperty]
        private string _catatan = string.Empty;

        private readonly KasirSession _sessionAktif;
        private readonly Action _onSuccess;

        public ClockOutViewModel(KasirSession session, Action onSuccess)
        {
            _sessionAktif = session;
            _onSuccess = onSuccess;

            NamaKasir = SessionManager.CurrentKasir?.Nama ?? "-";
            HitungRingkasanShift();
        }

        private void HitungRingkasanShift()
        {
            using var db = new AppDbContext();

            // Load data shift dari DB
            var shift = db.Shift.Find(_sessionAktif.ShiftId);
            NamaShift = shift?.NamaShift ?? "-";
            ModalAwal = _sessionAktif.ModalAwal;

            // Menggunakan DbSet 'Transaksi' sesuai schema KasirKu
            TotalOmzetTunai = db.Transaksi
                .Where(t => t.Tanggal >= _sessionAktif.WaktuLogin)
                .Sum(t => (decimal?)t.TotalHarga) ?? 0;

            TotalEkspektasi = ModalAwal + TotalOmzetTunai;
            HitungSelisih();
        }

        partial void OnTotalAktifFisikTextChanged(string value)
        {
            HitungSelisih();
        }

        private void HitungSelisih()
        {
            if (decimal.TryParse(TotalAktifFisikText, out decimal totalFisik))
            {
                Selisih = totalFisik - TotalEkspektasi;
            }
            else
            {
                Selisih = -TotalEkspektasi;
            }
        }

        [RelayCommand]
        public void SelesaikanShift()
        {
            if (!decimal.TryParse(TotalAktifFisikText, out decimal totalFisik) || totalFisik < 0)
            {
                MessageBox.Show("Nominal uang fisik di laci tidak valid!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();
            var sessionDb = db.KasirSession.Find(_sessionAktif.Id);

            if (sessionDb != null)
            {
                // Update ke properti KasirSession yang sesuai
                sessionDb.WaktuLogout = DateTime.Now;
                sessionDb.TotalTunaiSistem = TotalOmzetTunai;
                sessionDb.TotalTunaiFisik = totalFisik;
                sessionDb.SelisihKas = Selisih;
                sessionDb.CatatanSelisih = Catatan;
                sessionDb.IsClosed = true;

                // Tambahkan Audit Log
                db.AuditLog.Add(new AuditLog
                {
                    KasirId = sessionDb.KasirId,
                    Waktu = DateTime.Now,
                    JenisAksi = "CLOCK_OUT",
                    Keterangan = $"Selesai Shift ({NamaShift}). Modal Awal: Rp {ModalAwal:N0}, Omzet Tunai: Rp {TotalOmzetTunai:N0}, Uang Laci: Rp {totalFisik:N0}, Selisih: Rp {Selisih:N0}"
                });

                db.SaveChanges();
            }

            _onSuccess?.Invoke();
        }
    }
}