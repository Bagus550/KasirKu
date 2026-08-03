using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isPasswordVisible;

        public event EventHandler<Kasir>? LoginBerhasilEvent;

        public Func<Kasir, bool?>? RequestClockInHandler { get; set; }

        public LoginViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;
        }

        [RelayCommand]
        public void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                _dialogService.ShowWarning("Username dan Password wajib diisi!", "Peringatan");
                return;
            }

            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();

                var user = await db.Kasir.FirstOrDefaultAsync(k => k.Username.ToLower() == Username.Trim().ToLower());

                if (user == null || !PasswordHasherHelper.VerifyPassword(user, user.PasswordHash, Password))
                {
                    _dialogService.ShowError("Username atau Password salah!", "Login Gagal");
                    return;
                }

                _dialogService.ShowInfo($"Selamat datang, {user.Nama} ({user.Role})!", "Login Berhasil");

                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
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

                    await db.SaveChangesAsync();

                    SessionManager.SetSession(user, adminSession);
                    LoginBerhasilEvent?.Invoke(this, user);
                }
                else
                {
                    bool isClockInSuccess = RequestClockInHandler?.Invoke(user) ?? false;

                    if (isClockInSuccess)
                    {
                        LoginBerhasilEvent?.Invoke(this, user);
                    }
                    else
                    {
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