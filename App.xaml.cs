using KasirKu.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace KasirKu
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Buat database dan tabel Synchronous SEBELUM UI memuat ViewModel
            using (var context = new AppDbContext())
            {
                // Membuat file kasirku.db dan tabel Produk & Kasir jika belum ada
                context.Database.EnsureCreated();

                // Optimasi SQLite WAL Mode
                context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
                context.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
            }
        }
    }
}