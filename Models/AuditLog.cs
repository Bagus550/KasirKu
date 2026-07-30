using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int KasirId { get; set; }
        [ForeignKey("KasirId")]
        public virtual Kasir? Kasir { get; set; }

        public DateTime Waktu { get; set; } = DateTime.Now;

        [Required]
        public string JenisAksi { get; set; } = string.Empty; // LOGIN, LOGOUT, VOID_ITEM, BATAL_TRANSAKSI

        public string Keterangan { get; set; } = string.Empty;
    }
}