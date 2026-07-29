using KasirKu.ViewModels;
using System.Windows.Controls;

namespace KasirKu.Views
{
    public partial class LaporanView : UserControl
    {
        public LaporanView()
        {
            InitializeComponent();

            // Refresh data laporan setiap kali tab Laporan dibuka
            this.Loaded += (s, e) =>
            {
                if (DataContext is LaporanViewModel vm)
                {
                    vm.MuatLaporan();
                }
            };
        }
    }
}