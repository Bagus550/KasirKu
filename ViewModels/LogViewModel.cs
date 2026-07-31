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
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<FileInfo> _daftarLogFile = new();

        [ObservableProperty]
        private FileInfo? _selectedLogFile;

        [ObservableProperty]
        private string _isiLogContent = string.Empty;

        public LogViewModel(ILoggerService loggerService, IDialogService dialogService)
        {
            _loggerService = loggerService;
            _dialogService = dialogService;

            MuatDaftarLog();
        }

        partial void OnSelectedLogFileChanged(FileInfo? value)
        {
            if (value != null)
            {
                IsiLogContent = _loggerService.ReadLogFile(value.Name);
            }
            else
            {
                IsiLogContent = string.Empty;
            }
        }

        [RelayCommand]
        public void MuatDaftarLog()
        {
            var files = _loggerService.GetLogFiles();
            DaftarLogFile = new ObservableCollection<FileInfo>(files);

            if (DaftarLogFile.Any())
            {
                SelectedLogFile = DaftarLogFile.First();
            }
        }

        [RelayCommand]
        public void ExportLog()
        {
            if (SelectedLogFile == null || string.IsNullOrWhiteSpace(IsiLogContent))
            {
                _dialogService.ShowWarning("Pilih file log terlebih dahulu!", "Peringatan");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Log File (*.log)|*.log|Text File (*.txt)|*.txt",
                FileName = SelectedLogFile.Name,
                Title = "Ekspor File Log System"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, IsiLogContent);
                    _dialogService.ShowInfo($"File log berhasil diekspor ke:\n{saveFileDialog.FileName}", "Ekspor Sukses");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Gagal mengekspor file log: {ex.Message}", "Error Ekspor");
                }
            }
        }
    }
}