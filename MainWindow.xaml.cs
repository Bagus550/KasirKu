using KasirKu.Models;
using KasirKu.ViewModels;
using System.Windows;

namespace KasirKu
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hubungkan event saat login berhasil
            if (ViewLogin.DataContext is LoginViewModel loginVm)
            {
                loginVm.LoginBerhasilEvent += LoginVm_LoginBerhasilEvent;
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
                // Role Kasir: Hanya tampilkan Tab Kasir
                TabKasir.Visibility = Visibility.Visible;
                TabLaporan.Visibility = Visibility.Collapsed;
                TabProduk.Visibility = Visibility.Collapsed;
                TabAuditLog.Visibility = Visibility.Collapsed;

                TabUtama.SelectedItem = TabKasir; // Set tab aktif ke Kasir
            }
            else
            {
                // Role Admin: Hanya tampilkan Tab Laporan & Data Produk (Sembunyikan Tab Kasir)
                TabKasir.Visibility = Visibility.Collapsed;
                TabLaporan.Visibility = Visibility.Visible;
                TabProduk.Visibility = Visibility.Visible;
                TabAuditLog.Visibility = Visibility.Visible;

                TabUtama.SelectedItem = TabLaporan; // Set tab aktif default ke Laporan
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var currentKasir = Services.SessionManager.CurrentKasir;
            var currentSession = Services.SessionManager.CurrentSession;

            // A. JIKA USER ADALAH KASIR, TAMPILKAN POPUP CLOCK-OUT
            if (currentKasir != null && currentKasir.Role?.Equals("Kasir", StringComparison.OrdinalIgnoreCase) == true && currentSession != null)
            {
                var clockOutWin = new Views.ClockOutWindow(currentSession);
                bool? result = clockOutWin.ShowDialog();

                // Jika kasir membatalkan popup clock-out, batalkan proses logout
                if (result != true)
                {
                    return;
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
            Services.SessionManager.ClearSession();

            if (ViewLogin.DataContext is LoginViewModel loginVm)
            {
                loginVm.ResetForm();
            }

            GridUtama.Visibility = Visibility.Collapsed;
            ViewLogin.Visibility = Visibility.Visible;
        }
    }
}