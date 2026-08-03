using KasirKu.Data;
using KasirKu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasirKu.Services
{
    public class KasirService : IKasirService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly PasswordHasher<Kasir> _hasher;

        public KasirService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
            _hasher = new PasswordHasher<Kasir>();
        }

        // 1. Ambil Semua Data Kasir/Admin
        public async Task<List<Kasir>> GetAllKasirAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.Kasir.AsNoTracking().ToListAsync();
        }

        // 2. Tambah Akun Baru
        public async Task<bool> TambahKasirAsync(string nama, string username, string password, string role)
        {
            using var db = await _factory.CreateDbContextAsync();

            // Cek apakah username sudah ada
            bool usernameExists = await db.Kasir.AnyAsync(k => k.Username.ToLower() == username.ToLower());
            if (usernameExists)
            {
                throw new InvalidOperationException($"Username '{username}' sudah digunakan.");
            }

            var kasirBaru = new Kasir
            {
                Nama = nama,
                Username = username,
                Role = role
            };

            // Hashing password dengan instance kasirBaru yang valid
            kasirBaru.PasswordHash = _hasher.HashPassword(kasirBaru, password);

            db.Kasir.Add(kasirBaru);
            return await db.SaveChangesAsync() > 0;
        }

        // 3. Update Akun Kasir/Admin
        public async Task<bool> UpdateKasirAsync(int id, string nama, string username, string? password, string role)
        {
            using var db = await _factory.CreateDbContextAsync();

            var kasir = await db.Kasir.FindAsync(id);
            if (kasir == null)
            {
                throw new KeyNotFoundException("Data kasir tidak ditemukan.");
            }

            // Cek jika username diubah dan ternyata bentrok dengan user lain
            bool usernameExists = await db.Kasir.AnyAsync(k => k.Id != id && k.Username.ToLower() == username.ToLower());
            if (usernameExists)
            {
                throw new InvalidOperationException($"Username '{username}' sudah digunakan.");
            }

            kasir.Nama = nama;
            kasir.Username = username;
            kasir.Role = role;

            // Update password hanya jika diisi/diubah
            if (!string.IsNullOrWhiteSpace(password))
            {
                kasir.PasswordHash = _hasher.HashPassword(kasir, password);
            }

            return await db.SaveChangesAsync() > 0;
        }

        // 4. Hapus Akun Kasir
        public async Task<bool> HapusKasirAsync(int id)
        {
            using var db = await _factory.CreateDbContextAsync();

            var kasir = await db.Kasir.FindAsync(id);
            if (kasir == null) return false;

            db.Kasir.Remove(kasir);
            return await db.SaveChangesAsync() > 0;
        }
    }
}