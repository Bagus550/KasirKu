using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockInWindow : Window
    {
        public ClockInWindow(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService, Kasir kasir)
        {
            InitializeComponent();

            DataContext = new ClockInViewModel(contextFactory, dialogService, kasir, () =>
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