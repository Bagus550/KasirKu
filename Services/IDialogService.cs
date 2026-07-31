using System;
using System.Collections.Generic;
using System.Text;

namespace KasirKu.Services
{
    public interface IDialogService
    {
        void ShowInfo(string message, string title = "Informasi");
        void ShowWarning(string message, string title = "Peringatan");
        void ShowError(string message, string title = "Error");
        bool ShowConfirmation(string message, string title = "Konfirmasi");
    }
}
