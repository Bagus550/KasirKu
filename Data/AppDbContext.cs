using KasirKu.Models;
using Microsoft.Data.Sqlite;
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

        // DbSet Baru untuk Pelacakan Kasir & Shift
        public DbSet<Shift> Shift { get; set; }
        public DbSet<KasirSession> KasirSession { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath
            }.ToString();

            var connection = new SqliteConnection(connectionString);
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    PRAGMA journal_mode = WAL;
                    PRAGMA synchronous = NORMAL;
                ";
                command.ExecuteNonQuery();
            }

            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Data Kasir Awal
            modelBuilder.Entity<Kasir>().HasData(
                new Kasir { Id = 1, Nama = "Admin KasirKu", Username = "admin", PasswordHash = "admin123", Role = "Admin" },
                new Kasir { Id = 2, Nama = "Kasir Toko", Username = "kasir", PasswordHash = "kasir123", Role = "Kasir" }
            );

            // Seed Data Produk Awal
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

            // Seed Data Shift Awal (Fleksibel: Pagi, Siang, Sore/Malam, Malam 24 Jam)
            modelBuilder.Entity<Shift>().HasData(
                new Shift { Id = 1, NamaShift = "Shift Pagi", JamMulai = new TimeSpan(6, 0, 0), JamSelesai = new TimeSpan(12, 0, 0), IsAktif = true },
                new Shift { Id = 2, NamaShift = "Shift Siang", JamMulai = new TimeSpan(12, 0, 0), JamSelesai = new TimeSpan(17, 0, 0), IsAktif = true },
                new Shift { Id = 3, NamaShift = "Shift Sore/Malam", JamMulai = new TimeSpan(17, 0, 0), JamSelesai = new TimeSpan(22, 0, 0), IsAktif = true },
                new Shift { Id = 4, NamaShift = "Shift Malam (24 Jam)", JamMulai = new TimeSpan(22, 0, 0), JamSelesai = new TimeSpan(6, 0, 0), IsAktif = false }
            );
        }
    }
}