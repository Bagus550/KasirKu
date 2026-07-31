using KasirKu.Data; // Pastikan namespace AppDbContext di-import
using KasirKu.Services;
using KasirKu.ViewModels;
using KasirKu.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            // 3. INISIALISASI DATABASE AUTOMATIS (Memastikan tabel AuditLog & tabel lainnya dibuat)
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
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Membuat file database dan seluruh tabel (termasuk AuditLog) jika belum ada
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                // Catat ke logger jika terjadi kegagalan koneksi database saat startup
                var logger = ServiceProvider.GetService<ILoggerService>();
                logger?.LogError(ex, "Database Initialization");
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite("Data Source=kasirku.db"));

            services.AddSingleton<ILoggerService, LoggerService>();

            services.AddSingleton<IPrinterService, NullPrinterService>();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IProductService, ProductService>();

            services.AddTransient<LogViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<KasirViewModel>();
            services.AddTransient<ProdukViewModel>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<LaporanViewModel>();

            services.AddSingleton<MainWindow>();
        }

        private void SetupGlobalExceptionHandling()
        {
            // A. Catch unhandled errors on the WPF UI thread
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // B. Catch unhandled errors on background/non-UI threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // C. Catch unhandled errors in async Task / unobserved tasks
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogAndShowError(e.Exception, "UI Thread");

            // Tandai exception telah ditangani agar aplikasi TIDAK LANGSUNG CRASH
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

            // Tandai exception telah diamati
            e.SetObserved();
        }

        private void LogAndShowError(Exception ex, string source)
        {
            // Gunakan LoggerService untuk catat ke file .log
            var logger = ServiceProvider?.GetService<ILoggerService>();
            logger?.LogError(ex, source);

            // Tampilkan pesan error ramah ke user via IDialogService
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