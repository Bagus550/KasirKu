using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockOutWindow : Window
    {
        public bool IsClockOutSuccess { get; private set; } = false;

        public ClockOutWindow(KasirSession session, IDialogService dialogService)
        {
            InitializeComponent();

            // Mengirim 3 parameter sesuai signature ClockOutViewModel
            DataContext = new ClockOutViewModel(dialogService, session, () =>
            {
                IsClockOutSuccess = true;
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