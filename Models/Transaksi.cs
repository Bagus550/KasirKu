using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class Transaksi
    {
        public int Id { get; set; }
        public string NomorNota { get; set; } = string.Empty;
        public DateTime Tanggal { get; set; } = DateTime.Now;
        public decimal TotalHarga { get; set; }
        public decimal TotalBayar { get; set; }
        public decimal Kembalian { get; set; }

        public int? KasirSessionId { get; set; }
        [ForeignKey("KasirSessionId")]
        public virtual KasirSession? KasirSession { get; set; }

        public string NamaKasir { get; set; } = "Admin";

        // Relasi ke detail barang yang dibeli
        public List<DetailTransaksi> DetailTransaksi { get; set; } = new();
    }
}