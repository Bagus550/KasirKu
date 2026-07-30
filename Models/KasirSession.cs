using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KasirKu.Models
{
    public class KasirSession
    {
        [Key]
        public int Id { get; set; }

        public int KasirId { get; set; }
        [ForeignKey("KasirId")]
        public virtual Kasir? Kasir { get; set; }

        public int? ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public virtual Shift? Shift { get; set; }

        public DateTime WaktuLogin { get; set; } = DateTime.Now;
        public DateTime? WaktuLogout { get; set; }

        // Pencatatan Uang Kas & Rekonsiliasi
        public decimal ModalAwal { get; set; }
        public decimal TotalTunaiSistem { get; set; }
        public decimal TotalTunaiFisik { get; set; }
        public decimal SelisihKas { get; set; }
        public string? CatatanSelisih { get; set; }

        public bool IsClosed { get; set; } = false;

        // Relasi ke Transaksi
        public virtual ICollection<Transaksi> TransaksiList { get; set; } = new List<Transaksi>();
    }
}