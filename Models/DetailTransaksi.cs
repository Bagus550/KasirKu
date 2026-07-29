using KasirKu.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class DetailTransaksi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TransaksiId { get; set; }
        [ForeignKey("TransaksiId")]
        public Transaksi? Transaksi { get; set; }

        public int ProdukId { get; set; }
        [ForeignKey("ProdukId")]
        public Produk? Produk { get; set; }

        public string NamaProduk { get; set; } = string.Empty;
        public decimal HargaJual { get; set; }
        public int Jumlah { get; set; }
        public decimal Subtotal => HargaJual * Jumlah;
    }
}