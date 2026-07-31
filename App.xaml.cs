using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KasirKu.Data;
using KasirKu.Services;
using KasirKu.ViewModels;

namespace KasirKu
{
    public partial class App : Application
    {
        // Property global untuk mengakses ServiceProvider
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Inisialisasi Database (Kode asli milikmu)
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();
                context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
                context.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
            }

            // 2. Setup Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>();

            // Register Services
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<ITransactionService, TransactionService>();

            // Register ViewModels
            services.AddTransient<KasirViewModel>();
        }
    }
}