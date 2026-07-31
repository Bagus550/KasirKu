using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using KasirKu.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace KasirKu
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hubungkan DataContext LoginViewModel dan pasang handler ClockIn
            if (ViewLogin.DataContext is LoginViewModel loginVm)
            {
                loginVm.LoginBerhasilEvent += LoginVm_LoginBerhasilEvent;

                // Sambungkan delegate ClockInHandler ke View ClockInWindow
                loginVm.RequestClockInHandler = (kasir) =>
                {
                    var clockInWin = new ClockInWindow(kasir);
                    return clockInWin.ShowDialog();
                };
            }
        }

        private void LoginVm_LoginBerhasilEvent(object? sender, Kasir user)
        {
            // Sembunyikan layar login, tampilkan layar utama
            ViewLogin.Visibility = Visibility.Collapsed;
            GridUtama.Visibility = Visibility.Visible;

            TxtInfoUser.Text = $"User Aktif: {user.Nama} ({user.Role})";

            // Atur Hak Akses & Tab Default Berdasarkan Role
            if (user.Role.Equals("Kasir", StringComparison.OrdinalIgnoreCase))
            {
                // Inject KasirViewModel yang lengkap dengan Services ke ViewKasir
                ViewKasir.DataContext = App.ServiceProvider.GetRequiredService<KasirViewModel>();

                // Role Kasir: Hanya tampilkan Tab Kasir
                TabKasir.Visibility = Visibility.Visible;
                TabLaporan.Visibility = Visibility.Collapsed;
                TabProduk.Visibility = Visibility.Collapsed;
                TabAuditLog.Visibility = Visibility.Collapsed;

                TabUtama.SelectedItem = TabKasir; // Set tab aktif ke Kasir
            }
            else
            {
                // Inject ViewModel khusus Admin
                ViewProduk.DataContext = App.ServiceProvider.GetRequiredService<ProdukViewModel>();
                ViewAuditLog.DataContext = App.ServiceProvider.GetRequiredService<LogViewModel>();

                // Role Admin: Tampilkan Tab Laporan, Produk, & Audit Log
                TabKasir.Visibility = Visibility.Collapsed;
                TabLaporan.Visibility = Visibility.Visible;
                TabProduk.Visibility = Visibility.Visible;
                TabAuditLog.Visibility = Visibility.Visible;

                TabUtama.SelectedItem = TabLaporan; // Set tab aktif default ke Laporan
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var currentKasir = SessionManager.CurrentKasir;
            var currentSession = SessionManager.CurrentSession;

            // A. JIKA USER ADALAH KASIR, TAMPILKAN POPUP CLOCK-OUT
            if (currentKasir != null && currentKasir.Role?.Equals("Kasir", StringComparison.OrdinalIgnoreCase) == true && currentSession != null)
            {
                // Ambil IDialogService dari Dependency Injection Container
                var dialogService = App.ServiceProvider.GetRequiredService<IDialogService>();

                // Lewatkan dialogService sebagai parameter kedua ke ClockOutWindow
                var clockOutWin = new ClockOutWindow(currentSession, dialogService)
                {
                    Owner = this // Atur Owner agar posisi dialog berada di atas MainWindow
                };

                bool? result = clockOutWin.ShowDialog();

                if (result != true)
                {
                    return; // Jika user membatalkan Clock-Out, hentikan proses Logout
                }
            }
            // B. JIKA USER ADALAH ADMIN, UPDATE LOGOUT DIRECTLY
            else if (currentSession != null)
            {
                using var db = new Data.AppDbContext();
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