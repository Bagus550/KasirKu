using System.ComponentModel.DataAnnotations;

namespace KasirKu.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // Boleh plaintext/hash sesuai kebutuhan

        [Required]
        public string NamaLengkap { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Kasir"; // "Admin" atau "Kasir"

        public bool IsAktif { get; set; } = true;
    }
}