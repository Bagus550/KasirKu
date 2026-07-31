using KasirKu.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace KasirKu.Views
{
    public partial class LaporanView : UserControl
    {
        public LaporanView()
        {
            InitializeComponent();
        }

        // Method ini yang dicari oleh Loaded="UserControl_Loaded" di XAML
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LaporanViewModel vm)
            {
                await vm.MuatLaporanAsync();
            }
            else
            {
                MessageBox.Show("DataContext NULL / Tidak Terhubung ke LaporanViewModel!", "Debug Warning");
            }
        }
    }
}