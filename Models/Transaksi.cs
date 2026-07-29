using System;
using System.Collections.Generic;

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

        // Relasi ke detail barang yang dibeli
        public List<DetailTransaksi> DetailTransaksi { get; set; } = new();
    }
}