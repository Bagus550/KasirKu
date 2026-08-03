using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Linq;

namespace KasirKu.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        // Properti untuk mengontrol status tampil/sembunyi password
        [ObservableProperty]
        private bool _isPasswordVisible;

        // Event untuk memberitahu MainWindow/App Controller jika login berhasil
        public event EventHandler<Kasir>? LoginBerhasilEvent;

        // Action callback untuk membuka window ClockIn jika kasir biasa login (decoupling dari View)
        public Func<Kasir, bool?>? RequestClockInHandler { get; set; }

        // Constructor Utama: Menerima IDialogService dari DI Container
        public LoginViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        public void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        public void Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                _dialogService.ShowWarning("Username dan Password wajib diisi!", "Peringatan");
                return;
            }

            try
            {
                using var db = new AppDbContext();

                // 1. Cari user hanya berdasarkan Username
                var user = db.Kasir.FirstOrDefault(k => k.Username.ToLower() == Username.Trim().ToLower());

                // 2. Verifikasi Password dengan PasswordHasherHelper
                if (user == null || !PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, Password))
                {
                    _dialogService.ShowError("Username atau Password salah!", "Login Gagal");
                    return;
                }

                _dialogService.ShowInfo($"Selamat datang, {user.Nama} ({user.Role})!", "Login Berhasil");

                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    // JIKA ADMIN: Bypass ClockIn & Buat Session Khusus Admin
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

                    // Simpan sesi ke SessionManager
                    SessionManager.SetSession(user, adminSession);
                    LoginBerhasilEvent?.Invoke(this, user);
                }
                else
                {
                    // JIKA KASIR: Wajib Clock-In (Pilih Shift & Input Modal Awal)
                    bool isClockInSuccess = RequestClockInHandler?.Invoke(user) ?? false;

                    if (isClockInSuccess)
                    {
                        LoginBerhasilEvent?.Invoke(this, user);
                    }
                    else
                    {
                        // Jika Batal Clock-In, pastikan sesi dibersihkan
                        SessionManager.ClearSession();
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Terjadi kesalahan saat login: {ex.Message}", "Error Database");
            }
        }

        public void ResetForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            IsPasswordVisible = false;
        }
    }
}