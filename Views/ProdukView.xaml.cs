using KasirKu.ViewModels;
using System.Windows.Controls;

namespace KasirKu.Views
{
    public partial class ProdukView : UserControl
    {
        public ProdukView()
        {
            InitializeComponent();

            // Otomatis refresh data dari DB setiap kali tab Data Produk dibuka
            this.Loaded += async (s, e) =>
            {
                if (DataContext is ProdukViewModel vm)
                {
                    await vm.MuatDataProdukAsync();
                }
            };
        }
    }
}