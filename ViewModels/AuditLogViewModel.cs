using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
        private int _selectedTabIndex;

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

        [RelayCommand]
        private void ExportCsv()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = SelectedTabIndex == 0
                    ? $"Sesi_Kasir_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                    : $"Audit_Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();

                    if (SelectedTabIndex == 0)
                    {
                        // Export Sesi Kasir
                        sb.AppendLine("Nama Kasir;Shift Work;Waktu Login;Waktu Logout;Modal Awal;Total Omzet;Status");
                        foreach (var item in DaftarSesiKasir)
                        {
                            string kasir = item.Kasir?.Nama ?? "-";
                            string shift = item.Shift?.NamaShift ?? "Admin (Non-Shift)";
                            string login = item.WaktuLogin.ToString("dd/MM/yyyy HH:mm");
                            string logout = item.WaktuLogout.HasValue ? item.WaktuLogout.Value.ToString("dd/MM/yyyy HH:mm") : "Masih Aktif";
                            string modal = item.ModalAwal.ToString("F0");
                            string omzet = item.TotalTunaiSistem.ToString("F0");
                            string status = item.IsClosed ? "Selesai" : "Aktif";

                            sb.AppendLine($"\"{kasir}\";\"{shift}\";\"{login}\";\"{logout}\";{modal};{omzet};\"{status}\"");
                        }
                    }
                    else
                    {
                        // Export Log Aktivitas
                        sb.AppendLine("Waktu;Pengguna / Kasir;Jenis Aksi;Keterangan Audit");
                        foreach (var item in DaftarAuditLog)
                        {
                            string waktu = item.Waktu.ToString("dd/MM/yyyy HH:mm:ss");
                            string kasir = item.Kasir?.Nama ?? "System";
                            string aksi = item.JenisAksi;
                            string ket = item.Keterangan?.Replace("\"", "\"\"") ?? "";

                            sb.AppendLine($"\"{waktu}\";\"{kasir}\";\"{aksi}\";\"{ket}\"");
                        }
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    _dialogService?.ShowInfo("Export data CSV berhasil disimpan!", "Sukses");
                }
                catch (Exception ex)
                {
                    _dialogService?.ShowError($"Gagal melakukan export CSV: {ex.Message}", "Error");
                }
            }
        }

        [RelayCommand]
        private void BackupDatabase()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");

            if (!File.Exists(dbPath))
            {
                _dialogService?.ShowWarning("File database SQLite (kasirku.db) tidak ditemukan!", "Peringatan");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Database File (*.db)|*.db|SQLite File (*.sqlite)|*.sqlite",
                FileName = $"KasirKu_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.Copy(dbPath, saveFileDialog.FileName, overwrite: true);
                    _dialogService?.ShowInfo("Backup database berhasil dibuat!", "Sukses");
                }
                catch (Exception ex)
                {
                    _dialogService?.ShowError($"Gagal membuat backup database: {ex.Message}", "Error");
                }
            }
        }

        // Backward compatibility method
        public void MuatDataAudit()
        {
            _ = MuatLogAsync();
        }
    }
}