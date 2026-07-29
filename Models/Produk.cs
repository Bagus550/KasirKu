using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class Produk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nama { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; }

        [MaxLength(50)]
        public string Kategori { get; set; } = "Umum";

        [Column(TypeName = "decimal(18,2)")]
        public decimal HargaBeli { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HargaJual { get; set; }

        public int Stok { get; set; }
        public int StokMinimum { get; set; } = 5;
    }
}
