using System.ComponentModel.DataAnnotations;

namespace KasirKu.Models
{
    public class Kasir
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nama { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Kasir"; // Admin / Kasir
    }
}