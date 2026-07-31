using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KasirKu.ViewModels
{
    public partial class ClockInViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly Kasir _kasirAktif;
        private readonly Action _onSuccess;

        [ObservableProperty]
        private string _namaKasir = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Shift> _daftarShift = new();

        [ObservableProperty]
        private Shift? _shiftTerpilih;

        [ObservableProperty]
        private string _modalAwalText = "200000";
        public ClockInViewModel(IDialogService dialogService, Kasir kasir, Action onSuccess)
        {
            _dialogService = dialogService;
            _kasirAktif = kasir;
            _onSuccess = onSuccess;
            NamaKasir = kasir.Nama;

            MuatShift();
        }

        private void MuatShift()
        {
            try
            {
                using var db = new AppDbContext();
                var list = db.Shift.Where(s => s.IsAktif).ToList();
                DaftarShift = new ObservableCollection<Shift>(list);

                // Deteksi otomatis shift berdasarkan jam sekarang
                var skrg = DateTime.Now.TimeOfDay;
                ShiftTerpilih = list.FirstOrDefault(s => skrg >= s.JamMulai && skrg <= s.JamSelesai) ?? list.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat daftar shift: {ex.Message}");
            }
        }

        [RelayCommand]
        public void MulaiShift()
        {
            if (ShiftTerpilih == null)
            {
                _dialogService.ShowWarning("Pilih shift terlebih dahulu!", "Peringatan");
                return;
            }

            if (!decimal.TryParse(ModalAwalText, out decimal modalAwal) || modalAwal < 0)
            {
                _dialogService.ShowWarning("Nominal modal awal tidak valid!", "Peringatan");
                return;
            }

            try
            {
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
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memulai shift: {ex.Message}");
            }
        }
    }
}