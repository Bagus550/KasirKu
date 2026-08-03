using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KasirKu.Models;
using KasirKu.ViewModels;

namespace KasirKu.Views
{
    public partial class KasirView : UserControl
    {
        public KasirView()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not KasirViewModel vm || vm.ShortcutConfig == null) return;

            // Mendapatkan Key aktual (termasuk tombol sistem seperti F10/Alt)
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Abaikan tombol modifier tunggal
            if (key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt)
            {
                return;
            }

            var config = vm.ShortcutConfig;

            // 1. PROSES BAYAR (F12)
            if (key == config.ProsesBayar)
            {
                if (vm.ProsesBayarCommand.CanExecute(null))
                {
                    vm.ProsesBayarCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            // 2. HOLD TRANSAKSI (F2)
            if (key == config.HoldTransaksi)
            {
                if (vm.HoldTransaksiCommand.CanExecute(null))
                {
                    vm.HoldTransaksiCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            // 3. BATAL TRANSAKSI (F4)
            if (key == config.BatalTransaksi)
            {
                if (vm.BatalTransaksiCommand.CanExecute(null))
                {
                    vm.BatalTransaksiCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            // 4. FOKUS KE PENCARIAN BARCODE (F1)
            if (key == config.FokusPencarian)
            {
                cbBarcode?.Focus();
                e.Handled = true;
                return;
            }

            // 5. RESUME TRANSAKSI PERTAMA DARI HOLD (F3)
            if (key == config.ResumeTransaksi)
            {
                if (vm.DaftarHold.Count > 0 && vm.ResumeTransaksiCommand.CanExecute(vm.DaftarHold[0]))
                {
                    vm.ResumeTransaksiCommand.Execute(vm.DaftarHold[0]);
                    e.Handled = true;
                }
                return;
            }
        }

        private void ComboBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBoxItem item && item.DataContext is Produk produk)
            {
                if (DataContext is KasirViewModel vm)
                {
                    vm.PilihSuggestionCommand.Execute(produk);
                    e.Handled = true;
                }
            }
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;

            if (comboBox.IsDropDownOpen && (e.Key == Key.Down || e.Key == Key.Up))
            {
                int count = comboBox.Items.Count;
                if (count == 0) return;

                int currentIndex = comboBox.SelectedIndex;

                if (e.Key == Key.Down)
                {
                    currentIndex = (currentIndex + 1) < count ? currentIndex + 1 : 0;
                }
                else if (e.Key == Key.Up)
                {
                    currentIndex = (currentIndex - 1) >= 0 ? currentIndex - 1 : count - 1;
                }

                comboBox.SelectedIndex = currentIndex;
                e.Handled = true;
            }
        }
    }
}