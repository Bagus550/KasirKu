using KasirKu.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows;

namespace KasirKu.Services
{
    public class PrinterService
    {
        public static void CetakStruk(Transaksi transaksi, bool modeDemo = true)
        {
            // 1. Buat Format Teks Struk Sederhana
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("     ===== KASIRKU POS =====");
            sb.AppendLine($"Nota : {transaksi.NomorNota}");
            sb.AppendLine($"Tgl  : {transaksi.Tanggal:dd/MM/yyyy HH:mm}");
            sb.AppendLine("--------------------------------");

            foreach (var item in transaksi.DetailTransaksi)
            {
                sb.AppendLine($"{item.NamaProduk}");
                sb.AppendLine($"  {item.Jumlah} x {item.HargaJual:N0} = {item.Subtotal:N0}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Total   : Rp {transaksi.TotalHarga:N0}");
            sb.AppendLine($"Bayar   : Rp {transaksi.TotalBayar:N0}");
            sb.AppendLine($"Kembali : Rp {transaksi.Kembalian:N0}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine(" Terima Kasih Atas Kunjungan Anda! ");

            string teksStruk = sb.ToString();

            // 2. Jika Mode Demo / Skenario Testing (Tampilkan Pop-up di Layar Tanpa Lag)
            if (modeDemo)
            {
                MessageBox.Show(teksStruk, "📄 Preview Struk Pembayaran", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 3. Mode Cetak Fisik Ke Printer
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += (sender, e) =>
                    {
                        Graphics g = e.Graphics!;
                        using System.Drawing.Font fontBody = new System.Drawing.Font("Courier New", 8.0f, System.Drawing.FontStyle.Regular);

                        g.DrawString(teksStruk, fontBody, System.Drawing.Brushes.Black, 10, 10);
                    };
                    pd.Print();
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Gagal mencetak ke printer: {ex.Message}", "Printer Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
            });
        }
    }
}