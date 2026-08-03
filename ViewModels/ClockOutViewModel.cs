using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public partial class ClockOutViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;
        private readonly KasirSession _sessionAktif;
        private readonly Action _onSuccess;

        [ObservableProperty]
        private string _namaKasir = string.Empty;

        [ObservableProperty]
        private string _namaShift = string.Empty;

        [ObservableProperty]
        private decimal _modalAwal;

        [ObservableProperty]
        private decimal _totalOmzetTunai;

        [ObservableProperty]
        private decimal _totalEkspektasi;

        [ObservableProperty]
        private string _totalAktifFisikText = "0";

        [ObservableProperty]
        private decimal _selisih;

        [ObservableProperty]
        private string _catatan = string.Empty;

        public ClockOutViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService, KasirSession session, Action onSuccess)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;
            _sessionAktif = session;
            _onSuccess = onSuccess;

            NamaKasir = SessionManager.CurrentKasir?.Nama ?? "-";
            _ = HitungRingkasanShiftAsync();
        }

        private async Task HitungRingkasanShiftAsync()
        {
            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();

                var shift = await db.Shift.FindAsync(_sessionAktif.ShiftId);
                NamaShift = shift?.NamaShift ?? "-";
                ModalAwal = _sessionAktif.ModalAwal;

                TotalOmzetTunai = await db.Transaksi
                    .Where(t => t.Tanggal >= _sessionAktif.WaktuLogin)
                    .SumAsync(t => (decimal?)t.TotalHarga) ?? 0;

                TotalEkspektasi = ModalAwal + TotalOmzetTunai;
                HitungSelisih();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal menghitung ringkasan shift: {ex.Message}");
            }
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
        public async Task SelesaikanShiftAsync()
        {
            if (!decimal.TryParse(TotalAktifFisikText, out decimal totalFisik) || totalFisik < 0)
            {
                _dialogService.ShowWarning("Nominal uang fisik di laci tidak valid!", "Peringatan");
                return;
            }

            bool confirm = _dialogService.ShowConfirmation("Apakah Anda yakin ingin menyelesaikan shift dan melakukan Clock-Out?", "Konfirmasi Clock-Out");
            if (!confirm) return;

            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();
                var sessionDb = await db.KasirSession.FindAsync(_sessionAktif.Id);

                if (sessionDb != null)
                {
                    sessionDb.WaktuLogout = DateTime.Now;
                    sessionDb.TotalTunaiSistem = TotalOmzetTunai;
                    sessionDb.TotalTunaiFisik = totalFisik;
                    sessionDb.SelisihKas = Selisih;
                    sessionDb.CatatanSelisih = Catatan;
                    sessionDb.IsClosed = true;

                    db.AuditLog.Add(new AuditLog
                    {
                        KasirId = sessionDb.KasirId,
                        Waktu = DateTime.Now,
                        JenisAksi = "CLOCK_OUT",
                        Keterangan = $"Selesai Shift ({NamaShift}). Modal Awal: Rp {ModalAwal:N0}, Omzet Tunai: Rp {TotalOmzetTunai:N0}, Uang Laci: Rp {totalFisik:N0}, Selisih: Rp {Selisih:N0}"
                    });

                    await db.SaveChangesAsync();
                }

                _dialogService.ShowInfo("Berhasil menyelesaikan shift. Terima kasih!", "Informasi");
                SessionManager.ClearSession();
                _onSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal mengakhiri shift: {ex.Message}");
            }
        }
    }
}