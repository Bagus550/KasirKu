using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using KasirKu.ViewModels;
using KasirKu.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace KasirKu
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Inisialisasi Service Provider (Dependency Injection)
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // 2. Register Global Exception Handlers
            SetupGlobalExceptionHandling();

            // 3. Inisialisasi Database (Jalankan Migrasi & PRAGMA)
            InitializeDatabase();

            // 4. Tampilkan MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            var logger = ServiceProvider.GetService<ILoggerService>();
            logger?.LogInfo("Aplikasi KasirKu berhasil dijalankan.");
        }

        private void InitializeDatabase()
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                using var db = factory.CreateDbContext();

                // Jalankan Migrasi
                db.Database.Migrate();

                // Jalankan PRAGMA SQLite
                db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
                db.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");

                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Kasir>();

                var admin = db.Kasir.FirstOrDefault(k => k.Username == "Admin");
                if (admin != null)
                {
                    admin.PasswordHash = hasher.HashPassword(admin, "admin123");
                }

                var kasir = db.Kasir.FirstOrDefault(k => k.Username == "Kasir");
                if (kasir != null)
                {
                    kasir.PasswordHash = hasher.HashPassword(kasir, "kasir123");
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider.GetService<ILoggerService>();
                logger?.LogError(ex, "Database Initialization (Migrate)");
                throw; // Lempar exception agar langsung terdeteksi jika migrasi gagal
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Set Path Database SQLite Absolut
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "kasirku.db");
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Services
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddSingleton<IPrinterService, PrinterService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IShortcutService, ShortcutService>();

            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IKasirService, KasirService>();
            

            // ViewModels
            services.AddTransient<LogViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<KasirViewModel>();
            services.AddTransient<ProdukViewModel>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<LaporanViewModel>();
            services.AddTransient<KelolaAkunViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
        }

        private void SetupGlobalExceptionHandling()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogAndShowError(e.Exception, "UI Thread");
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogAndShowError(ex, "Domain Thread");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogAndShowError(e.Exception, "Async Task");
            e.SetObserved();
        }

        private void LogAndShowError(Exception ex, string source)
        {
            var logger = ServiceProvider?.GetService<ILoggerService>();
            logger?.LogError(ex, source);

            var dialogService = ServiceProvider?.GetService<IDialogService>();
            if (dialogService != null)
            {
                dialogService.ShowError(
                    $"Terjadi kesalahan yang tidak terduga ({source}):\n{ex.Message}\n\nDetail telah dicatat ke log.",
                    "Aplikasi Mengalami Masalah"
                );
            }
            else
            {
                MessageBox.Show(
                    $"Terjadi kesalahan tidak terduga: {ex.Message}",
                    "System Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}