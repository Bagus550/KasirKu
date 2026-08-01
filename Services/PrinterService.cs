using KasirKu.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;

namespace KasirKu.Services
{
    public enum PrinterPaperSize
    {
        Paper58mm,
        Paper80mm
    }

    public interface IPrinterService
    {
        Task<bool> CetakStrukAsync(Transaksi transaksi, PrinterPaperSize paperSize = PrinterPaperSize.Paper58mm, string? namaPrinter = null);
    }

    public class PrinterService : IPrinterService
    {
        private readonly IDialogService _dialogService;

        public PrinterService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        /// <summary>
        /// Mencetak struk transaksi secara asynchronous tanpa memblokir UI thread.
        /// </summary>
        public async Task<bool> CetakStrukAsync(Transaksi transaksi, PrinterPaperSize paperSize = PrinterPaperSize.Paper58mm, string? namaPrinter = null)
        {
            if (transaksi == null)
            {
                _dialogService.ShowWarning("Data transaksi kosong, tidak dapat mencetak struk.", "Peringatan Cetak");
                return false;
            }

            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                _dialogService.ShowWarning(
                    "Tidak ada driver printer yang terinstal di sistem Windows Anda. Cetak dilewati.",
                    "Cetak Nota Dilewati"
                );
                return false;
            }

            // Jalankan proses driver printer di background thread
            var result = await Task.Run(() =>
            {
                try
                {
                    using var printDoc = new PrintDocument();

                    if (!string.IsNullOrWhiteSpace(namaPrinter))
                    {
                        printDoc.PrinterSettings.PrinterName = namaPrinter;
                    }

                    // Cek ketersediaan/keabsahan printer
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        return (Success: false, ErrorMessage: $"Printer '{namaPrinter ?? "Default"}' tidak ditemukan atau tidak siap.");
                    }

                    // Hilangkan margin bawaan driver Windows agar tidak terpotong di printer thermal
                    printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                    // Event handler tata letak/desain struk thermal
                    printDoc.PrintPage += (sender, e) => GambarkanStruk(e, transaksi, paperSize);

                    printDoc.Print();
                    return (Success: true, ErrorMessage: string.Empty);
                }
                catch (Exception ex)
                {
                    return (Success: false, ErrorMessage: ex.Message);
                }
            });

            // Tampilkan UI Dialog secara aman di Main UI Thread jika ada kegagalan
            if (!result.Success)
            {
                _dialogService.ShowWarning(
                    $"Transaksi berhasil disimpan, namun nota gagal dicetak.\nDetail: {result.ErrorMessage}",
                    "Cetak Nota Dilewati"
                );
            }

            return result.Success;
        }

        /// <summary>
        /// Menggambar layout fisik nota/struk kasir.
        /// </summary>
        private void GambarkanStruk(PrintPageEventArgs e, Transaksi transaksi, PrinterPaperSize paperSize)
        {
            if (e.Graphics == null) return;

            var g = e.Graphics;

            // --- KONFIGURASI DINAMIS SESUAI UKURAN KERTAS ---
            float width = (paperSize == PrinterPaperSize.Paper58mm) ? 195f : 280f;
            int maxChars = (paperSize == PrinterPaperSize.Paper58mm) ? 32 : 48;
            float leftMargin = 2f;

            using var fontHeader = new Font("Courier New", 9.5f, FontStyle.Bold);
            using var fontRegular = new Font("Courier New", 8f, FontStyle.Regular);
            using var fontBold = new Font("Courier New", 8f, FontStyle.Bold);

            float y = 5f;
            float lineSpace = 13f;

            // 1. Header Toko (Center)
            DrawTextCenter(g, fontHeader, "KASIRKU STORE", width, ref y, lineSpace + 2);
            DrawTextCenter(g, fontRegular, "Jl. Raya Utama No. 123", width, ref y, lineSpace + 4);

            DrawSeparator(g, fontRegular, maxChars, leftMargin, ref y, lineSpace);

            // 2. Info Transaksi
            g.DrawString($"Nota   : {transaksi.NomorNota}", fontRegular, Brushes.Black, leftMargin, y);
            y += lineSpace;
            g.DrawString($"Tgl    : {transaksi.Tanggal:dd/MM/yyyy HH:mm}", fontRegular, Brushes.Black, leftMargin, y);
            y += lineSpace;
            g.DrawString($"Kasir  : {transaksi.NamaKasir}", fontRegular, Brushes.Black, leftMargin, y);
            y += lineSpace;

            DrawSeparator(g, fontRegular, maxChars, leftMargin, ref y, lineSpace);

            // 3. Daftar Item / Detail Transaksi
            if (transaksi.DetailTransaksi != null)
            {
                foreach (var item in transaksi.DetailTransaksi)
                {
                    string namaProduk = item.NamaProduk.Length > maxChars
                        ? item.NamaProduk.Substring(0, maxChars - 3) + "..."
                        : item.NamaProduk;

                    g.DrawString(namaProduk, fontBold, Brushes.Black, leftMargin, y);
                    y += lineSpace;

                    string qtyPrice = $" {item.Jumlah} x {item.HargaJual:N0}";
                    string subtotal = item.Subtotal.ToString("N0");

                    g.DrawString(qtyPrice, fontRegular, Brushes.Black, leftMargin, y);

                    // Align Right Subtotal
                    float subtotalWidth = g.MeasureString(subtotal, fontRegular).Width;
                    g.DrawString(subtotal, fontRegular, Brushes.Black, width - subtotalWidth, y);
                    y += lineSpace;
                }
            }

            DrawSeparator(g, fontRegular, maxChars, leftMargin, ref y, lineSpace);

            // 4. Total & Pembayaran
            DrawLineTotal(g, fontBold, width, leftMargin, "TOTAL   :", transaksi.TotalHarga, ref y, lineSpace);
            DrawLineTotal(g, fontRegular, width, leftMargin, "BAYAR   :", transaksi.TotalBayar, ref y, lineSpace);
            DrawLineTotal(g, fontRegular, width, leftMargin, "KEMBALI :", transaksi.Kembalian, ref y, lineSpace);

            // 5. Footer
            DrawSeparator(g, fontRegular, maxChars, leftMargin, ref y, lineSpace);
            DrawTextCenter(g, fontRegular, "Terima Kasih Atas Kunjungan Anda!", width, ref y, lineSpace);
        }

        #region Helpers Layout

        private void DrawTextCenter(Graphics g, Font font, string text, float width, ref float y, float lineSpace)
        {
            float textWidth = g.MeasureString(text, font).Width;
            float x = (width - textWidth) / 2;
            if (x < 0) x = 0;
            g.DrawString(text, font, Brushes.Black, x, y);
            y += lineSpace;
        }

        private void DrawSeparator(Graphics g, Font font, int maxChars, float leftMargin, ref float y, float lineSpace)
        {
            g.DrawString(new string('-', maxChars), font, Brushes.Black, leftMargin, y);
            y += lineSpace;
        }

        private void DrawLineTotal(Graphics g, Font font, float width, float leftMargin, string label, decimal amount, ref float y, float lineSpace)
        {
            string valStr = $"Rp {amount:N0}";
            g.DrawString(label, font, Brushes.Black, leftMargin, y);

            float valWidth = g.MeasureString(valStr, font).Width;
            g.DrawString(valStr, font, Brushes.Black, width - valWidth, y);
            y += lineSpace;
        }

        #endregion
    }
}