using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KasirKu.Models;
using KasirKu.Services;

namespace KasirKu.Views
{
    public partial class PengaturanShortcutWindow : Window
    {
        private readonly IShortcutService _shortcutService;
        public ShortcutSetting CurrentSettings { get; private set; }

        private readonly List<Key> _availableKeys = new()
        {
            Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
            Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
            Key.Insert, Key.End, Key.Home
        };

        public PengaturanShortcutWindow(IShortcutService shortcutService)
        {
            InitializeComponent();
            _shortcutService = shortcutService;
            CurrentSettings = _shortcutService.LoadShortcuts();

            PopulateComboBoxes();
            LoadCurrentValues();
        }

        private void PopulateComboBoxes()
        {
            CmbFokusPencarian.ItemsSource = _availableKeys;
            CmbHold.ItemsSource = _availableKeys;
            CmbResume.ItemsSource = _availableKeys;
            CmbBatal.ItemsSource = _availableKeys;
            CmbBayar.ItemsSource = _availableKeys;
        }

        private void LoadCurrentValues()
        {
            CmbFokusPencarian.SelectedItem = CurrentSettings.FokusPencarian;
            CmbHold.SelectedItem = CurrentSettings.HoldTransaksi;
            CmbResume.SelectedItem = CurrentSettings.ResumeTransaksi;
            CmbBatal.SelectedItem = CurrentSettings.BatalTransaksi;
            CmbBayar.SelectedItem = CurrentSettings.ProsesBayar;
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Ambil nilai pilihan dari ComboBox
            var fokus = (Key)(CmbFokusPencarian.SelectedItem ?? Key.F1);
            var hold = (Key)(CmbHold.SelectedItem ?? Key.F2);
            var resume = (Key)(CmbResume.SelectedItem ?? Key.F3);
            var batal = (Key)(CmbBatal.SelectedItem ?? Key.F4);
            var bayar = (Key)(CmbBayar.SelectedItem ?? Key.F12);

            // 2. Validasi: Pastikan tidak ada tombol shortcut yang sama/bentrok
            var selectedKeys = new List<Key> { fokus, hold, resume, batal, bayar };
            if (selectedKeys.Distinct().Count() < selectedKeys.Count)
            {
                MessageBox.Show(
                    "Setiap aksi harus menggunakan tombol shortcut yang berbeda! Silakan periksa kembali pilihan Anda.",
                    "Peringatan Shortcut Bentrok",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // 3. Simpan setting jika validasi lolos
            CurrentSettings.FokusPencarian = fokus;
            CurrentSettings.HoldTransaksi = hold;
            CurrentSettings.ResumeTransaksi = resume;
            CurrentSettings.BatalTransaksi = batal;
            CurrentSettings.ProsesBayar = bayar;

            _shortcutService.SaveShortcuts(CurrentSettings);

            DialogResult = true;
            Close();
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}