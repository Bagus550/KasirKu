using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.Views; // <-- Pastikan namespace Views di-import
using System;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        // Menyimpan data kasir/user yang sedang login
        public static Kasir? UserLoginAktif { get; private set; }

        // Event untuk memberitahu MainWindow jika login berhasil
        public event EventHandler<Kasir>? LoginBerhasilEvent;

        [RelayCommand]
        public void Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Username dan Password wajib diisi!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            // Cek kredensial kasir di DB
            var user = db.Kasir.FirstOrDefault(k =>
                k.Username.ToLower() == Username.Trim().ToLower() &&
                k.PasswordHash == Password);

            if (user == null)
            {
                MessageBox.Show("Username atau Password salah!", "Login Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UserLoginAktif = user;
            MessageBox.Show($"Selamat datang, {user.Nama} ({user.Role})!", "Login Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);

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
                // JIKA KASIR: WajibClock-In (Pilih Shift & Input Modal Awal)
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
    }
}