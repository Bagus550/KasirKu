using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockInWindow : Window
    {
        public ClockInWindow(Kasir kasir, IDialogService dialogService)
        {
            InitializeComponent();

            DataContext = new ClockInViewModel(dialogService, kasir, () =>
            {
                DialogResult = true;
                Close();
            });
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}