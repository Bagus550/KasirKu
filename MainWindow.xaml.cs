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

                TabUtama.SelectedItem = TabKasir; // Set tab aktif ke Kasir
            }
            else
            {
                // Role Admin: Hanya tampilkan Tab Laporan & Data Produk (Sembunyikan Tab Kasir)
                TabKasir.Visibility = Visibility.Collapsed;
                TabLaporan.Visibility = Visibility.Visible;
                TabProduk.Visibility = Visibility.Visible;

                TabUtama.SelectedItem = TabLaporan; // Set tab aktif default ke Laporan
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            GridUtama.Visibility = Visibility.Collapsed;
            ViewLogin.Visibility = Visibility.Visible;
        }
    }
}