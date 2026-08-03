using System.Windows;
using System.Windows.Controls;
using KasirKu.ViewModels;

namespace KasirKu.Views
{
    public partial class LoginView : UserControl
    {
        private bool _isSyncingPassword = false;

        public LoginView()
        {
            InitializeComponent();
        }

        private void TxtPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isSyncingPassword) return;

            if (DataContext is LoginViewModel vm)
            {
                _isSyncingPassword = true;
                vm.Password = TxtPasswordBox.Password;
                TxtVisiblePassword.Text = TxtPasswordBox.Password;
                _isSyncingPassword = false;
            }
        }

        private void TxtVisiblePassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSyncingPassword) return;

            if (DataContext is LoginViewModel vm)
            {
                _isSyncingPassword = true;
                TxtPasswordBox.Password = TxtVisiblePassword.Text;
                vm.Password = TxtVisiblePassword.Text;
                _isSyncingPassword = false;
            }
        }
    }
}