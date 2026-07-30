using KasirKu.Models;
using System.Text;
using System.Windows;

namespace KasirKu.Services
{
    public static class PrinterService
    {
        public static void CetakStruk(Transaksi transaksi)
        {
            var sb = new StringBuilder();
            sb.AppendLine("      ===== KASIRKU POS =====");
            sb.AppendLine($"Nota  : {transaksi.NomorNota}");
            sb.AppendLine($"Tgl   : {transaksi.Tanggal:dd/MM/yyyy HH:mm}");

            // TAMBAHKAN BARIS INI:
            sb.AppendLine($"Kasir : {transaksi.NamaKasir}");

            sb.AppendLine("--------------------------------");

            foreach (var item in transaksi.DetailTransaksi)
            {
                sb.AppendLine($"{item.NamaProduk}");
                sb.AppendLine($"  {item.Jumlah} x {item.HargaJual:N0} = {item.Subtotal:N0}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Total : Rp {transaksi.TotalHarga:N0}");
            sb.AppendLine($"Bayar : Rp {transaksi.TotalBayar:N0}");
            sb.AppendLine($"Kembali : Rp {transaksi.Kembalian:N0}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine("  Terima Kasih Atas Kunjungan Anda!");

            // Tampilkan Preview Struk
            MessageBox.Show(sb.ToString(), "Preview Struk Pembayaran", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}