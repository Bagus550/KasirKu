using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Windows;

namespace KasirKu.Views
{
    public partial class ClockInWindow : Window
    {
        private readonly Kasir _kasir;

        public ClockInWindow(Kasir kasir)
        {
            InitializeComponent();
            _kasir = kasir;
            TxtKasirInfo.Text = $"Kasir: {_kasir.Nama} ({_kasir.Username})";
        }

        private void BtnBukaShift_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtModalAwal.Text, out decimal modalAwal) || modalAwal < 0)
            {
                MessageBox.Show("Masukkan nominal modal awal yang valid!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            // Menggunakan KasirSession sesuai entity model kamu
            var session = new KasirSession
            {
                KasirId = _kasir.Id,
                WaktuLogin = DateTime.Now,
                ModalAwal = modalAwal,
                TotalTunaiSistem = modalAwal, // Inisialisasi awal
                IsClosed = false
            };

            db.KasirSession.Add(session);
            db.SaveChanges();

            // Simpan Session Aktif ke Memory Global
            SessionManager.CurrentKasir = _kasir;
            SessionManager.CurrentSession = session;

            DialogResult = true;
            Close();
        }
    }
}