using KasirKu.Services;
using KasirKu.ViewModels;
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

            // 3. Tampilkan MainWindow
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddSingleton<ILoggerService, LoggerService>();
            services.AddTransient<LogViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPrinterService, PrinterService>();
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<IProductService, ProductService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<KasirViewModel>();
            services.AddTransient<ProdukViewModel>();

            // Views
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