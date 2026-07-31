using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace KasirKu.ViewModels
{
    public partial class LogViewModel : ObservableObject
    {
        private readonly ILoggerService _loggerService;
        private readonly IDialogService? _dialogService;

        [ObservableProperty]
        private ObservableCollection<FileInfo> _daftarLogFile = new();

        [ObservableProperty]
        private FileInfo? _selectedLogFile;

        [ObservableProperty]
        private string _isiLogContent = string.Empty;

        public LogViewModel(ILoggerService loggerService, IDialogService? dialogService = null)
        {
            _loggerService = loggerService;
            _dialogService = dialogService;

            MuatDaftarLog();
        }

        partial void OnSelectedLogFileChanged(FileInfo? value)
        {
            if (value != null)
            {
                try
                {
                    IsiLogContent = _loggerService.ReadLogFile(value.Name);
                }
                catch (Exception ex)
                {
                    IsiLogContent = $"[Gagal Membaca File Log]: {ex.Message}";
                }
            }
            else
            {
                IsiLogContent = string.Empty;
            }
        }

        [RelayCommand]
        public void MuatDaftarLog()
        {
            try
            {
                var previousFileName = SelectedLogFile?.Name;

                var files = _loggerService.GetLogFiles() ?? Enumerable.Empty<FileInfo>();
                DaftarLogFile = new ObservableCollection<FileInfo>(files);

                if (DaftarLogFile.Any())
                {
                    var matchedFile = DaftarLogFile.FirstOrDefault(f => f.Name == previousFileName);

                    // Trigger pembaruan UI
                    SelectedLogFile = matchedFile ?? DaftarLogFile.First();

                    if (SelectedLogFile != null)
                    {
                        IsiLogContent = _loggerService.ReadLogFile(SelectedLogFile.Name);
                    }
                }
                else
                {
                    SelectedLogFile = null;
                    IsiLogContent = "Belum ada file log sistem yang tercatat di folder Logs.";
                }
            }
            catch (Exception ex)
            {
                _dialogService?.ShowError($"Gagal memuat daftar file log: {ex.Message}", "Error Log");
            }
        }

        [RelayCommand]
        public void ExportLog()
        {
            if (SelectedLogFile == null || string.IsNullOrWhiteSpace(IsiLogContent))
            {
                _dialogService?.ShowWarning("Pilih file log terlebih dahulu!", "Peringatan");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Log File (*.log)|*.log|Text File (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = SelectedLogFile.Name,
                Title = "Ekspor File Log System"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, IsiLogContent);
                    _dialogService?.ShowInfo($"File log berhasil diekspor ke:\n{saveFileDialog.FileName}", "Ekspor Sukses");
                }
                catch (Exception ex)
                {
                    _dialogService?.ShowError($"Gagal mengekspor file log: {ex.Message}", "Error Ekspor");
                }
            }
        }
    }
}