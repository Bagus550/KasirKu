using KasirKu.Models;
using KasirKu.ViewModels;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockOutWindow : Window
    {
        public bool IsClockOutSuccess { get; private set; } = false;

        public ClockOutWindow(KasirSession session)
        {
            InitializeComponent();
            DataContext = new ClockOutViewModel(session, () =>
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