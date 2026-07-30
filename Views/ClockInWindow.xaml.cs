using KasirKu.Models;
using KasirKu.ViewModels;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockInWindow : Window
    {
        public ClockInWindow(Kasir kasir)
        {
            InitializeComponent();
            DataContext = new ClockInViewModel(kasir, () =>
            {
                DialogResult = true;
                Close();
            });
        }
    }
}