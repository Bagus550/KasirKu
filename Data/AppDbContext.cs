using KasirKu.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace KasirKu.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produk> Produk { get; set; }
        public DbSet<Kasir> Kasir { get; set; }
        public DbSet<Transaksi> Transaksi { get; set; }
        public DbSet<DetailTransaksi> DetailTransaksi { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Simpan database SQLite di folder lokal aplikasi
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Data Awal untuk testing
            modelBuilder.Entity<Kasir>().HasData(
                new Kasir { Id = 1, Nama = "Admin KasirKu", Username = "admin", PasswordHash = "admin123", Role = "Admin" },
                new Kasir { Id = 2, Nama = "Kasir Toko", Username = "kasir", PasswordHash = "kasir123", Role = "Kasir" }
            );

            modelBuilder.Entity<Produk>().HasData(
                new Produk { Id = 1, Nama = "Beras 5kg", SKU = "BRS01", Kategori = "Sembako", HargaBeli = 60000, HargaJual = 68000, Stok = 20, StokMinimum = 5 },
                new Produk { Id = 2, Nama = "Minyak Goreng 1L", SKU = "MYK01", Kategori = "Sembako", HargaBeli = 14000, HargaJual = 16000, Stok = 30, StokMinimum = 10 }
            );
        }
    }
}