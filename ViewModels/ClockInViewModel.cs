using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public partial class ClockInViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
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

        public ClockInViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService, Kasir kasir, Action onSuccess)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;
            _kasirAktif = kasir;
            _onSuccess = onSuccess;
            NamaKasir = kasir.Nama;

            _ = MuatShiftAsync();
        }

        private async Task MuatShiftAsync()
        {
            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();
                var list = await db.Shift.Where(s => s.IsAktif).ToListAsync();
                DaftarShift = new ObservableCollection<Shift>(list);

                var skrg = DateTime.Now.TimeOfDay;
                ShiftTerpilih = list.FirstOrDefault(s => skrg >= s.JamMulai && skrg <= s.JamSelesai) ?? list.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat daftar shift: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task MulaiShiftAsync()
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
                using var db = await _contextFactory.CreateDbContextAsync();

                var session = new KasirSession
                {
                    KasirId = _kasirAktif.Id,
                    ShiftId = ShiftTerpilih.Id,
                    WaktuLogin = DateTime.Now,
                    ModalAwal = modalAwal,
                    IsClosed = false
                };

                db.KasirSession.Add(session);

                db.AuditLog.Add(new AuditLog
                {
                    KasirId = _kasirAktif.Id,
                    Waktu = DateTime.Now,
                    JenisAksi = "LOGIN_SHIFT",
                    Keterangan = $"Login di {ShiftTerpilih.NamaShift} dengan Modal Awal Rp {modalAwal:N0}"
                });

                await db.SaveChangesAsync();

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