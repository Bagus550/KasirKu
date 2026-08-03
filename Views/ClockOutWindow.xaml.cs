using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockOutWindow : Window
    {
        public bool IsClockOutSuccess { get; private set; } = false;

        public ClockOutWindow(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService, KasirSession session)
        {
            InitializeComponent();

            DataContext = new ClockOutViewModel(contextFactory, dialogService, session, () =>
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