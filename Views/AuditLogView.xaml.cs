using KasirKu.ViewModels;
using System.Windows.Controls;

namespace KasirKu.Views
{
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();

            var vm = new AuditLogViewModel();
            DataContext = vm;

            // Pemicu Auto-Refresh setiap kali Admin klik Tab Audit Log
            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue && DataContext is AuditLogViewModel activeVm)
                {
                    activeVm.MuatDataAudit();
                }
            };
        }
    }
}