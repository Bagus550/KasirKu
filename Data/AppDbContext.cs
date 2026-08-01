using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace KasirKu.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Produk> Produk { get; set; } = null!;
        public DbSet<Kasir> Kasir { get; set; } = null!;
        public DbSet<Transaksi> Transaksi { get; set; } = null!;
        public DbSet<DetailTransaksi> DetailTransaksi { get; set; } = null!;

        // DbSet untuk Pelacakan Kasir & Shift
        public DbSet<Shift> Shift { get; set; } = null!;
        public DbSet<KasirSession> KasirSession { get; set; } = null!;
        public DbSet<AuditLog> AuditLog { get; set; } = null!;

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");

                // Serahkan pembuatan & penutupan koneksi SQLite ke EF Core sepenuhnya
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dummy object hanya untuk dipakai parameter HashPassword
            var dummyAdmin = new Kasir { Id = 1, Username = "admin" };
            var dummyKasir = new Kasir { Id = 2, Username = "kasir" };

            // 1. Seed Data Kasir Awal dengan PASSWORD HASHED (Priority 1)
            modelBuilder.Entity<Kasir>().HasData(
                new Kasir
                {
                    Id = 1,
                    Nama = "Bagus Setiawan",
                    Username = "Bagus",
                    PasswordHash = PasswordHasherHelper.HashPassword(dummyAdmin, "admin123"),
                    Role = "Admin"
                },
                new Kasir
                {
                    Id = 2,
                    Nama = "Anton Wijaya",
                    Username = "Anton",
                    PasswordHash = PasswordHasherHelper.HashPassword(dummyKasir, "kasir123"),
                    Role = "Kasir"
                }
            );

            // 2. Seed Data Produk Awal
            modelBuilder.Entity<Produk>().HasData(
                new Produk { Id = 1, Nama = "Beras 5kg", SKU = "BRS01", Kategori = "Sembako", HargaBeli = 60000, HargaJual = 68000, Stok = 20, StokMinimum = 5 },
                new Produk { Id = 2, Nama = "Minyak Goreng 1L", SKU = "MYK01", Kategori = "Sembako", HargaBeli = 14000, HargaJual = 16000, Stok = 30, StokMinimum = 10 },
                new Produk { Id = 3, Nama = "Tepung Terigu 1kg", SKU = "TPG01", Kategori = "Sembako", HargaBeli = 10000, HargaJual = 12000, Stok = 30, StokMinimum = 5 },
                new Produk { Id = 4, Nama = "Tepung Tapioka 1Kg", SKU = "TPG02", Kategori = "Sembako", HargaBeli = 14000, HargaJual = 15000, Stok = 30, StokMinimum = 10 },
                new Produk { Id = 5, Nama = "Indomie Goreng 1 Bks", SKU = "IND01", Kategori = "Makanan Instan", HargaBeli = 3000, HargaJual = 3500, Stok = 80, StokMinimum = 10 },
                new Produk { Id = 6, Nama = "Indomie Kuah Ayam Bawang 1 Bks", SKU = "IND02", Kategori = "Makanan Instan", HargaBeli = 3000, HargaJual = 3500, Stok = 80, StokMinimum = 10 },
                new Produk { Id = 7, Nama = "Mie Sedaap Soto 1 Bks", SKU = "SDP01", Kategori = "Makanan Instan", HargaBeli = 3000, HargaJual = 3500, Stok = 80, StokMinimum = 10 },
                new Produk { Id = 8, Nama = "Garam Daun 250g", SKU = "GRM01", Kategori = "Penyedap", HargaBeli = 2000, HargaJual = 2500, Stok = 30, StokMinimum = 5 },
                new Produk { Id = 9, Nama = "Sasa Penyedap Rasa 250g", SKU = "SSA01", Kategori = "Penyedap", HargaBeli = 13200, HargaJual = 14000, Stok = 50, StokMinimum = 10 }
            );

            // 3. Seed Data Shift Awal
            modelBuilder.Entity<Shift>().HasData(
                new Shift { Id = 1, NamaShift = "Shift Pagi", JamMulai = new TimeSpan(6, 0, 0), JamSelesai = new TimeSpan(12, 0, 0), IsAktif = true },
                new Shift { Id = 2, NamaShift = "Shift Siang", JamMulai = new TimeSpan(12, 0, 0), JamSelesai = new TimeSpan(17, 0, 0), IsAktif = true },
                new Shift { Id = 3, NamaShift = "Shift Sore/Malam", JamMulai = new TimeSpan(17, 0, 0), JamSelesai = new TimeSpan(22, 0, 0), IsAktif = true },
                new Shift { Id = 4, NamaShift = "Shift Malam (24 Jam)", JamMulai = new TimeSpan(22, 0, 0), JamSelesai = new TimeSpan(6, 0, 0), IsAktif = false }
            );
        }
    }
}