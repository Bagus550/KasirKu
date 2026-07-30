using System;
using System.ComponentModel.DataAnnotations;

namespace KasirKu.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NamaShift { get; set; } = string.Empty; // Pagi, Siang, Sore, Malam

        public TimeSpan JamMulai { get; set; } // Contoh: 06:00:00

        public TimeSpan JamSelesai { get; set; } // Contoh: 12:00:00

        public bool IsAktif { get; set; } = true;
    }
}