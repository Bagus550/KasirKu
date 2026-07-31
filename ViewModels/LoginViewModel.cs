using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.Views;
using System;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDialogService? _dialogService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        // Menyimpan data kasir/user yang sedang login
        public static Kasir? UserLoginAktif { get; private set; }

        // Event untuk memberitahu MainWindow jika login berhasil
        public event EventHandler<Kasir>? LoginBerhasilEvent;

        // Constructor Injection
        public LoginViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        // Parameterless constructor untuk XAML Designer / Fallback
        public LoginViewModel()
        {
        }

        [RelayCommand]
        public void Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ShowMessage("Username dan Password wajib diisi!", "Peringatan", MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            // 1. Cari user hanya berdasarkan Username
            var user = db.Kasir.FirstOrDefault(k => k.Username.ToLower() == Username.Trim().ToLower());

            // 2. Verifikasi Password dengan PasswordHasherHelper
            if (user == null || !PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, Password))
            {
                ShowMessage("Username atau Password salah!", "Login Gagal", MessageBoxImage.Error);
                return;
            }

            UserLoginAktif = user;
            ShowMessage($"Selamat datang, {user.Nama} ({user.Role})!", "Login Berhasil", MessageBoxImage.Information);

            if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // JIKA ADMIN: Bypass ClockIn & Buat Session Dummy Khusus Admin
                var adminSession = new KasirSession
                {
                    KasirId = user.Id,
                    ShiftId = null,
                    WaktuLogin = DateTime.Now,
                    ModalAwal = 0,
                    IsClosed = false
                };

                db.KasirSession.Add(adminSession);

                db.AuditLog.Add(new AuditLog
                {
                    KasirId = user.Id,
                    Waktu = DateTime.Now,
                    JenisAksi = "LOGIN_ADMIN",
                    Keterangan = "Admin Login Sistem"
                });

                db.SaveChanges();

                SessionManager.SetSession(user, adminSession);
                LoginBerhasilEvent?.Invoke(this, user);
            }
            else
            {
                // JIKA KASIR: Wajib Clock-In (Pilih Shift & Input Modal Awal)
                var clockInWin = new ClockInWindow(user);
                bool? isClockInSuccess = clockInWin.ShowDialog();

                if (isClockInSuccess == true)
                {
                    LoginBerhasilEvent?.Invoke(this, user);
                }
                else
                {
                    UserLoginAktif = null;
                }
            }
        }

        public void ResetForm()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        // Helper untuk penanganan pesan yang fleksibel (IDialogService atau Fallback MessageBox)
        private void ShowMessage(string message, string title, MessageBoxImage icon)
        {
            if (_dialogService != null)
            {
                switch (icon)
                {
                    case MessageBoxImage.Warning:
                        _dialogService.ShowWarning(message, title);
                        break;
                    case MessageBoxImage.Error:
                        _dialogService.ShowError(message, title);
                        break;
                    default:
                        _dialogService.ShowInfo(message, title);
                        break;
                }
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, icon);
            }
        }
    }
}