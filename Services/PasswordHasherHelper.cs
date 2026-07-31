using Microsoft.AspNetCore.Identity;
using KasirKu.Models;

namespace KasirKu.Services
{
    public static class PasswordHasherHelper
    {
        private static readonly PasswordHasher<Kasir> _hasher = new();

        /// <summary>
        /// Mengubah password plaintext menjadi string Hash + Salt acak
        /// </summary>
        public static string HashPassword(Kasir kasir, string plainPassword)
        {
            return _hasher.HashPassword(kasir, plainPassword);
        }

        /// <summary>
        /// Memverifikasi apakah password yang diinput cocok dengan hash di database
        /// </summary>
        public static bool VerifyPassword(Kasir kasir, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            var result = _hasher.VerifyHashedPassword(kasir, hashedPassword, providedPassword);

            // Mengembalikan true jika password cocok (Success atau SuccessRehashNeeded)
            return result != PasswordVerificationResult.Failed;
        }
    }
}