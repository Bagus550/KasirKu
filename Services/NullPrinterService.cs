using KasirKu.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KasirKu.Services
{
    public class NullPrinterService : IPrinterService
    {
        public Task<bool> CetakStrukAsync(Transaksi transaksi, PrinterPaperSize paperSize = PrinterPaperSize.Paper58mm, string? namaPrinter = null)
        {
            Debug.WriteLine($"[SIMULASI PRINTER] Struk Nota #{transaksi?.NomorNota} berhasil dicetak.");
            Debug.WriteLine($"[SIMULASI PRINTER] Total: Rp {transaksi?.TotalHarga:N0}, Ukuran Kertas: {paperSize}");

            // Mengembalikan nilai true seolah-olah proses cetak sukses
            return Task.FromResult(true);
        }
    }
}