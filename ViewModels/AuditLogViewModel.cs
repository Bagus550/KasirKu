using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        private string _selectedJenisAksi = "Semua Aksi";

        [ObservableProperty]
        private ObservableCollection<AuditLog> _daftarAuditLog = new();

        [ObservableProperty]
        private ObservableCollection<KasirSession> _daftarSesiKasir = new();

        [ObservableProperty]
        private bool _isLoading;

        // Daftar Pilihan Filter untuk ComboBox
        public ObservableCollection<string> DaftarFilterAksi { get; } = new()
        {
            "Semua Aksi",
            "⚠️ Aksi Sensitif Only",
            "TAMBAH_PRODUK",
            "EDIT_PRODUK",
            "HAPUS_PRODUK",
            "BATAL_TRANSAKSI",
            "PROSES_TRANSAKSI",
            "CLOCK_IN",
            "CLOCK_OUT"
        };

        public AuditLogViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService? dialogService = null)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;

            _ = MuatLogAsync();
        }

        [RelayCommand]
        public async Task MuatLogAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                await using var db = await _contextFactory.CreateDbContextAsync();

                DateTime tglMulai = TanggalAwal.Date;
                DateTime tglSelesai = TanggalAkhir.Date.AddDays(1);

                // 1. Query dasar untuk Audit Log
                var logQuery = db.AuditLog
                    .AsNoTracking()
                    .Include(a => a.Kasir)
                    .Where(a => a.Waktu >= tglMulai && a.Waktu < tglSelesai);

                if (SelectedJenisAksi == "⚠️ Aksi Sensitif Only")
                {
                    var aksiSensitif = new[] { "TAMBAH_PRODUK", "EDIT_PRODUK", "HAPUS_PRODUK", "BATAL_TRANSAKSI" };
                    logQuery = logQuery.Where(a => aksiSensitif.Contains(a.JenisAksi));
                }
                else if (SelectedJenisAksi != "Semua Aksi" && !string.IsNullOrWhiteSpace(SelectedJenisAksi))
                {
                    logQuery = logQuery.Where(a => a.JenisAksi == SelectedJenisAksi);
                }

                var logs = await logQuery
                    .OrderByDescending(a => a.Waktu)
                    .ToListAsync();

                // 2. Ambil Sesi Shift Kasir (Hanya Role 'Kasir')
                var sessions = await db.KasirSession
                    .AsNoTracking()
                    .Include(s => s.Kasir)
                    .Include(s => s.Shift)
                    .Where(s => s.WaktuLogin >= tglMulai && s.WaktuLogin < tglSelesai)
                    .Where(s => s.Kasir != null && s.Kasir.Role == "Kasir")
                    .OrderByDescending(s => s.WaktuLogin)
                    .ToListAsync();

                DaftarAuditLog = new ObservableCollection<AuditLog>(logs);
                DaftarSesiKasir = new ObservableCollection<KasirSession>(sessions);
            }
            catch (Exception ex)
            {
                _dialogService?.ShowError($"Gagal memuat log audit: {ex.Message}", "Error Database");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Backward compatibility method
        public void MuatDataAudit()
        {
            _ = MuatLogAsync();
        }
    }
}