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
    public partial class AuditLogViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService? _dialogService;

        [ObservableProperty]
        private DateTime _tanggalAwal = DateTime.Today;

        [ObservableProperty]
        private DateTime _tanggalAkhir = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<AuditLog> _daftarAuditLog = new();

        [ObservableProperty]
        private ObservableCollection<KasirSession> _daftarSesiKasir = new();

        public AuditLogViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService? dialogService = null)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;

            _ = MuatLogAsync();
        }

        [RelayCommand]
        public async Task MuatLogAsync()
        {
            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();

                DateTime tglMulai = TanggalAwal.Date;
                DateTime tglSelesai = TanggalAkhir.Date.AddDays(1);

                // 1. Ambil Log Aktivitas (Sistem / Global - Admin & Kasir Tetap Tercatat di Sini)
                var logs = await db.AuditLog
                    .AsNoTracking()
                    .Include(a => a.Kasir)
                    .Where(a => a.Waktu >= tglMulai && a.Waktu < tglSelesai)
                    .OrderByDescending(a => a.Waktu)
                    .ToListAsync();

                DaftarAuditLog = new ObservableCollection<AuditLog>(logs);

                // 2. Ambil Sesi Shift Kasir (Hanya Role 'Kasir' yang Tampil, Admin Diabaikan)
                var sessions = await db.KasirSession
                    .AsNoTracking()
                    .Include(s => s.Kasir)
                    .Include(s => s.Shift)
                    .Where(s => s.WaktuLogin >= tglMulai && s.WaktuLogin < tglSelesai)
                    .Where(s => s.Kasir != null && s.Kasir.Role == "Kasir")
                    .OrderByDescending(s => s.WaktuLogin)
                    .ToListAsync();

                DaftarSesiKasir = new ObservableCollection<KasirSession>(sessions);
            }
            catch (Exception ex)
            {
                _dialogService?.ShowError($"Gagal memuat log audit: {ex.Message}");
            }
        }

        // Backward compatibility method
        public void MuatDataAudit()
        {
            _ = MuatLogAsync();
        }
    }
}