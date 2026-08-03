using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KasirKu.ViewModels
{
    public class KelolaAkunViewModel : INotifyPropertyChanged
    {
        private readonly IKasirService _kasirService;
        private readonly IDialogService? _dialogService;

        // Properti Input Form
        private int _id;
        private string _nama = string.Empty;
        private string _username = string.Empty;
        private string _selectedRole = "Kasir";
        private bool _isEditMode;
        private Kasir? _selectedKasir;

        public ObservableCollection<Kasir> DaftarKasir { get; set; } = new();

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Nama
        {
            get => _nama;
            set { _nama = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        public Kasir? SelectedKasir
        {
            get => _selectedKasir;
            set
            {
                _selectedKasir = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand SimpanCommand { get; }
        public ICommand BatalCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand HapusCommand { get; }
        public ICommand RefreshCommand { get; }

        public KelolaAkunViewModel(IKasirService kasirService, IDialogService? dialogService = null)
        {
            _kasirService = kasirService;
            _dialogService = dialogService;

            SimpanCommand = new RelayCommand(async (param) => await SimpanAsync(param));
            BatalCommand = new RelayCommand((_) => ResetForm());
            EditCommand = new RelayCommand((param) => SetEditMode(param as Kasir));
            HapusCommand = new RelayCommand(async (param) => await HapusAsync(param as Kasir));
            RefreshCommand = new RelayCommand(async (_) => await LoadDataAsync());

            _ = LoadDataAsync();
        }

        public async Task LoadDataAsync()
        {
            try
            {
                var list = await _kasirService.GetAllKasirAsync();
                DaftarKasir.Clear();
                foreach (var item in list)
                {
                    DaftarKasir.Add(item);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Gagal memuat data kasir: {ex.Message}");
            }
        }

        private async Task SimpanAsync(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string password = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Nama) || string.IsNullOrWhiteSpace(Username))
            {
                ShowWarning("Nama dan Username tidak boleh kosong.");
                return;
            }

            if (!IsEditMode && string.IsNullOrWhiteSpace(password))
            {
                ShowWarning("Password wajib diisi untuk akun baru.");
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    // Update User
                    bool success = await _kasirService.UpdateKasirAsync(Id, Nama, Username, password, SelectedRole);
                    if (success)
                    {
                        ShowInfo("Data pengguna berhasil diperbarui.");
                        ResetForm(passwordBox);
                        await LoadDataAsync();
                    }
                }
                else
                {
                    // Tambah User Baru
                    bool success = await _kasirService.TambahKasirAsync(Nama, Username, password, SelectedRole);
                    if (success)
                    {
                        ShowInfo("Pengguna baru berhasil ditambahkan.");
                        ResetForm(passwordBox);
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Gagal menyimpan data: {ex.Message}");
            }
        }

        private void SetEditMode(Kasir? kasir)
        {
            if (kasir == null) return;

            Id = kasir.Id;
            Nama = kasir.Nama;
            Username = kasir.Username;
            SelectedRole = kasir.Role;
            IsEditMode = true;
        }

        private async Task HapusAsync(Kasir? kasir)
        {
            if (kasir == null) return;

            bool confirm = ConfirmMessage($"Apakah Anda yakin ingin menghapus akun '{kasir.Nama}'?");
            if (!confirm) return;

            try
            {
                bool success = await _kasirService.HapusKasirAsync(kasir.Id);
                if (success)
                {
                    ShowInfo("Akun berhasil dihapus.");
                    if (Id == kasir.Id) ResetForm();
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Gagal menghapus akun: {ex.Message}");
            }
        }

        private void ResetForm(PasswordBox? passwordBox = null)
        {
            Id = 0;
            Nama = string.Empty;
            Username = string.Empty;
            SelectedRole = "Kasir";
            IsEditMode = false;
            SelectedKasir = null;

            if (passwordBox != null)
            {
                passwordBox.Password = string.Empty;
            }
        }

        private void ShowInfo(string msg)
        {
            if (_dialogService != null) _dialogService.ShowInfo(msg, "Informasi");
            else MessageBox.Show(msg, "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowWarning(string msg)
        {
            if (_dialogService != null) _dialogService.ShowWarning(msg, "Peringatan");
            else MessageBox.Show(msg, "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowError(string msg)
        {
            if (_dialogService != null) _dialogService.ShowError(msg, "Kesalahan");
            else MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool ConfirmMessage(string msg)
        {
            if (_dialogService != null) return _dialogService.ShowConfirmation(msg, "Konfirmasi");
            return MessageBox.Show(msg, "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper RelayCommand Sederhana
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}