using KasirKu.ViewModels;
using System.Windows.Controls;

namespace KasirKu.Views
{
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                if (DataContext is AuditLogViewModel vm)
                {
                    await vm.MuatLogAsync();
                }
            };
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}