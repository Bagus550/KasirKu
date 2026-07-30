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
                new Produk { Id = 2, Nama = "Minyak Goreng 1L", SKU = "MYK01", Kategori = "Sembako", HargaBeli = 14000, HargaJual = 16000, Stok = 30, StokMinimum = 10 }
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