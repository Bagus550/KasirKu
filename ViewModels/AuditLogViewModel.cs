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
    public partial class AuditLogViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _tanggalAwal = DateTime.Today;

        [ObservableProperty]
        private DateTime _tanggalAkhir = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<AuditLog> _daftarAuditLog = new();

        [ObservableProperty]
        private ObservableCollection<KasirSession> _daftarSesiKasir = new();

        public AuditLogViewModel()
        {
            MuatDataAudit();
        }

        [RelayCommand]
        public void MuatDataAudit()
        {
            using var db = new AppDbContext();

            DateTime tglMulai = TanggalAwal.Date;
            DateTime tglSelesai = TanggalAkhir.Date.AddDays(1).AddTicks(-1);

            // 1. Ambil Log Aktivitas (Pakai AsNoTracking agar selalu ambil data paling segar dari SQLite)
            var logs = db.AuditLog
                .AsNoTracking()
                .Include(a => a.Kasir)
                .Where(a => a.Waktu >= tglMulai && a.Waktu <= tglSelesai)
                .OrderByDescending(a => a.Waktu)
                .ToList();

            DaftarAuditLog = new ObservableCollection<AuditLog>(logs);

            // 2. Ambil Sesi Shift Kasir (Pakai AsNoTracking)
            var sessions = db.KasirSession
                .AsNoTracking()
                .Include(s => s.Kasir)
                .Include(s => s.Shift)
                .Where(s => s.WaktuLogin >= tglMulai && s.WaktuLogin <= tglSelesai)
                .OrderByDescending(s => s.WaktuLogin)
                .ToList();

            DaftarSesiKasir = new ObservableCollection<KasirSession>(sessions);
        }
    }
}