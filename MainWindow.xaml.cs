using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using KasirKu.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KasirKu
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 1. Ambil LoginViewModel dari DI Container & assign ke ViewLogin
            var loginVm = App.ServiceProvider.GetRequiredService<LoginViewModel>();
            ViewLogin.DataContext = loginVm;

            // 2. Hubungkan event Login
            loginVm.LoginBerhasilEvent += LoginVm_LoginBerhasilEvent;

            // 3. Sambungkan delegate ClockInHandler ke View ClockInWindow
            loginVm.RequestClockInHandler = (kasir) =>
            {
                var contextFactory = App.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                var dialogService = App.ServiceProvider.GetRequiredService<IDialogService>();

                var clockInWin = new ClockInWindow(contextFactory, dialogService, kasir)
                {
                    Owner = this
                };
                return clockInWin.ShowDialog();
            };

            // 4. Memicu Auto Refresh saat Tab dipilih/diklik
            TabUtama.SelectionChanged += TabUtama_SelectionChanged;
        }

        private async void LoginVm_LoginBerhasilEvent(object? sender, Kasir user)
        {
            ViewLogin.Visibility = Visibility.Collapsed;
            GridUtama.Visibility = Visibility.Visible;

            TxtInfoUser.Text = $"User Aktif: {user.Nama} ({user.Role})";

            if (user.Role.Equals("Kasir", StringComparison.OrdinalIgnoreCase))
            {
                ViewKasir.DataContext = App.ServiceProvider.GetRequiredService<KasirViewModel>();

                TabKasir.Visibility = Visibility.Visible;
                TabLaporan.Visibility = Visibility.Collapsed;
                TabProduk.Visibility = Visibility.Collapsed;
                TabKelolaAkun.Visibility = Visibility.Collapsed;
                TabAuditLog.Visibility = Visibility.Collapsed;
                TabSystemLog.Visibility = Visibility.Collapsed;

                TabUtama.SelectedItem = TabKasir;
            }
            else
            {
                // Assign ViewModel untuk fitur Admin
                var laporanVm = App.ServiceProvider.GetRequiredService<LaporanViewModel>();
                ViewLaporan.DataContext = laporanVm;

                ViewProduk.DataContext = App.ServiceProvider.GetRequiredService<ProdukViewModel>();
                ViewKelolaAkun.DataContext = App.ServiceProvider.GetRequiredService<KelolaAkunViewModel>(); // Assign KelolaAkunVM
                ViewAuditLog.DataContext = App.ServiceProvider.GetRequiredService<AuditLogViewModel>();

                var logVm = App.ServiceProvider.GetRequiredService<LogViewModel>();
                ViewSystemLog.DataContext = logVm;

                // Atur Visibilitas Tab
                TabKasir.Visibility = Visibility.Collapsed;
                TabLaporan.Visibility = Visibility.Visible;
                TabProduk.Visibility = Visibility.Visible;
                TabKelolaAkun.Visibility = Visibility.Visible; // Tampilkan untuk Admin
                TabAuditLog.Visibility = Visibility.Visible;
                TabSystemLog.Visibility = Visibility.Visible;

                TabUtama.SelectedItem = TabLaporan;

                await laporanVm.MuatLaporanAsync();
            }
        }

        private async void TabUtama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != TabUtama) return;

            if (TabUtama.SelectedItem == TabLaporan && ViewLaporan.DataContext is LaporanViewModel laporanVm)
            {
                await laporanVm.MuatLaporanAsync();
            }
            else if (TabUtama.SelectedItem == TabKelolaAkun && ViewKelolaAkun.DataContext is KelolaAkunViewModel kelolaAkunVm)
            {
                await kelolaAkunVm.LoadDataAsync();
            }
            else if (TabUtama.SelectedItem == TabAuditLog && ViewAuditLog.DataContext is AuditLogViewModel auditVm)
            {
                await auditVm.MuatLogAsync();
            }
            else if (TabUtama.SelectedItem == TabSystemLog && ViewSystemLog.DataContext is LogViewModel logVm)
            {
                logVm.MuatDaftarLog();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var currentKasir = SessionManager.CurrentKasir;
            var currentSession = SessionManager.CurrentSession;
            var dialogService = App.ServiceProvider.GetRequiredService<IDialogService>();
            var contextFactory = App.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

            // A. JIKA USER ADALAH KASIR, TAMPILKAN POPUP CLOCK-OUT
            if (currentKasir != null && currentKasir.Role?.Equals("Kasir", StringComparison.OrdinalIgnoreCase) == true && currentSession != null)
            {
                var clockOutWin = new ClockOutWindow(contextFactory, dialogService, currentSession)
                {
                    Owner = this
                };

                bool? result = clockOutWin.ShowDialog();

                if (result != true)
                {
                    return; // Jika user membatalkan Clock-Out, hentikan proses Logout
                }
            }
            // B. JIKA USER ADALAH ADMIN, TAMPILKAN DIALOG KONFIRMASI DULU
            else
            {
                bool konfirmasi = dialogService.ShowConfirmation(
                    "Apakah Anda yakin ingin keluar dari akun Admin?",
                    "Konfirmasi Logout"
                );

                if (!konfirmasi)
                {
                    return;
                }

                if (currentSession != null)
                {
                    using var db = contextFactory.CreateDbContext();

                    var sessionDb = db.KasirSession.Find(currentSession.Id);
                    if (sessionDb != null && !sessionDb.IsClosed)
                    {
                        sessionDb.WaktuLogout = DateTime.Now;
                        sessionDb.IsClosed = true;

                        db.AuditLog.Add(new Models.AuditLog
                        {
                            KasirId = sessionDb.KasirId,
                            Waktu = DateTime.Now,
                            JenisAksi = "LOGOUT",
                            Keterangan = $"Admin {currentKasir?.Nama} telah Logout."
                        });

                        db.SaveChanges();
                    }
                }
            }

            // CLEAR SESSION GLOBAL & RESET TAMPILAN
            SessionManager.ClearSession();

            if (ViewLogin.DataContext is LoginViewModel loginVm)
            {
                loginVm.ResetForm();
            }

            GridUtama.Visibility = Visibility.Collapsed;
            ViewLogin.Visibility = Visibility.Visible;
        }
    }
}