using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class Produk
    {
        public int Id { get; set; }
        public string? SKU { get; set; }
        public string Nama { get; set; } = string.Empty;
        public string? Kategori { get; set; } = "Umum";
        public decimal HargaBeli { get; set; } = 0;
        public decimal HargaJual { get; set; } = 0;

        public int Stok { get; set; }
        public int StokMinimum { get; set; } = 0;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public override string ToString() => Nama;
    }
}