using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class ClockInViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _namaKasir = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Shift> _daftarShift = new();

        [ObservableProperty]
        private Shift? _shiftTerpilih;

        [ObservableProperty]
        private string _modalAwalText = "200000"; // Default modal awal (bisa diubah)

        private readonly Kasir _kasirAktif;
        private readonly Action _onSuccess;

        public ClockInViewModel(Kasir kasir, Action onSuccess)
        {
            _kasirAktif = kasir;
            _onSuccess = onSuccess;
            NamaKasir = kasir.Nama;

            MuatShift();
        }

        private void MuatShift()
        {
            using var db = new AppDbContext();
            var list = db.Shift.Where(s => s.IsAktif).ToList();
            DaftarShift = new ObservableCollection<Shift>(list);

            // Deteksi otomatis shift berdasarkan jam sekarang
            var skrg = DateTime.Now.TimeOfDay;
            ShiftTerpilih = list.FirstOrDefault(s => skrg >= s.JamMulai && skrg <= s.JamSelesai) ?? list.FirstOrDefault();
        }

        [RelayCommand]
        public void MulaiShift()
        {
            if (ShiftTerpilih == null)
            {
                MessageBox.Show("Pilih shift terlebih dahulu!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(ModalAwalText, out decimal modalAwal) || modalAwal < 0)
            {
                MessageBox.Show("Nominal modal awal tidak valid!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            // Buat KasirSession Baru
            var session = new KasirSession
            {
                KasirId = _kasirAktif.Id,
                ShiftId = ShiftTerpilih.Id,
                WaktuLogin = DateTime.Now,
                ModalAwal = modalAwal,
                IsClosed = false
            };

            db.KasirSession.Add(session);

            // Catat Log Audit Login
            db.AuditLog.Add(new AuditLog
            {
                KasirId = _kasirAktif.Id,
                Waktu = DateTime.Now,
                JenisAksi = "LOGIN_SHIFT",
                Keterangan = $"Login di {ShiftTerpilih.NamaShift} dengan Modal Awal Rp {modalAwal:N0}"
            });

            db.SaveChanges();

            // Set State Sesi Global
            SessionManager.SetSession(_kasirAktif, session);

            _onSuccess?.Invoke();
        }
    }
}