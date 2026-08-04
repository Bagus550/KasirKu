using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KasirKu.ViewModels
{
    public class HoldTransactionModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString()[..5].ToUpper();
        public DateTime Waktu { get; set; } = DateTime.Now;
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalHarga => Items.Sum(x => x.Subtotal);
    }

    public partial class KasirViewModel : ObservableObject, IDisposable
    {
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly IPrinterService _printerService;
        private readonly IShortcutService _shortcutService;

        private bool _isUpdatingFromSelection = false;
        private CancellationTokenSource? _ctsSearch;

        [ObservableProperty]
        private ObservableCollection<CartItem> _keranjang = new();

        [ObservableProperty]
        private ObservableCollection<HoldTransactionModel> _daftarHold = new();

        [ObservableProperty]
        private string _inputBarcode = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Produk> _daftarSuggestionProduk = new();

        [ObservableProperty]
        private Produk? _selectedProdukSuggestion;

        [ObservableProperty]
        private bool _isSuggestionOpen;

        [ObservableProperty]
        private ShortcutSetting _shortcutConfig;

        [ObservableProperty]
        private decimal _totalHarga;

        [ObservableProperty]
        private decimal _totalBayar;

        [ObservableProperty]
        private decimal _kembalian;

        public KasirViewModel(
            ITransactionService transactionService,
            IProductService productService,
            IDialogService dialogService,
            IPrinterService printerService,
            IShortcutService shortcutService)
        {
            _transactionService = transactionService;
            _productService = productService;
            _dialogService = dialogService;
            _printerService = printerService;
            _shortcutService = shortcutService;

            _shortcutConfig = _shortcutService.LoadShortcuts();

            SessionManager.SessionCleared += OnSessionCleared;
            HitungTotal();
        }

        private void OnSessionCleared(object? sender, EventArgs e)
        {
            ResetKeranjang();
            DaftarHold.Clear();
            ResetInputSuggestion();
        }

        partial void OnInputBarcodeChanged(string value)
        {
            if (_isUpdatingFromSelection) return;

            _ctsSearch?.Cancel();
            _ctsSearch = new CancellationTokenSource();

            _ = CariSuggestionProdukAsync(value, _ctsSearch.Token);
        }

        public ObservableCollection<decimal> DaftarPecahanRupiah { get; } = new()
        {
            2000,
            5000,
            10000,
            20000,
            50000,
            100000
        };

        [RelayCommand]
        private void PilihPecahanUang(decimal nominal)
        {
            TotalBayar = nominal;
        }

        [RelayCommand]
        private void BayarUangPas()
        {
            TotalBayar = TotalHarga;
        }

        [RelayCommand]
        public void BukaPengaturanShortcut()
        {
            var dialog = new Views.PengaturanShortcutWindow(_shortcutService)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                ShortcutConfig = dialog.CurrentSettings;
                _dialogService.ShowInfo("Shortcut berhasil diperbarui!", "Pengaturan");
            }
        }

        private async Task CariSuggestionProdukAsync(string keyword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                TutupSuggestion();
                return;
            }

            try
            {
                await Task.Delay(250, cancellationToken);

                var list = await _productService.SearchProductsAsync(keyword, limit: 8);

                if (cancellationToken.IsCancellationRequested) return;

                _isUpdatingFromSelection = true;

                DaftarSuggestionProduk.Clear();
                if (list != null && list.Any())
                {
                    foreach (var prod in list)
                    {
                        DaftarSuggestionProduk.Add(prod);
                    }
                    IsSuggestionOpen = true;
                }
                else
                {
                    IsSuggestionOpen = false;
                }

                _isUpdatingFromSelection = false;
            }
            catch (OperationCanceledException)
            {
                // Disregard canceled searches
            }
            catch
            {
                TutupSuggestion();
            }
        }

        partial void OnSelectedProdukSuggestionChanged(Produk? value)
        {
            if (_isUpdatingFromSelection) return;

            if (value != null)
            {
                _isUpdatingFromSelection = true;
                InputBarcode = value.Nama;
                _isUpdatingFromSelection = false;
            }
        }

        [RelayCommand]
        public void PilihSuggestion(Produk? produk)
        {
            if (produk == null) return;

            TambahProdukKeKeranjang(produk);
            ResetInputSuggestion();
        }

        [RelayCommand]
        public async Task TambahBarangAsync()
        {
            if (SelectedProdukSuggestion != null)
            {
                TambahProdukKeKeranjang(SelectedProdukSuggestion);
                ResetInputSuggestion();
                return;
            }

            if (string.IsNullOrWhiteSpace(InputBarcode)) return;

            try
            {
                var produk = await _productService.GetProductBySkuOrNameAsync(InputBarcode);

                if (produk == null)
                {
                    _dialogService.ShowWarning($"Produk dengan SKU/Nama '{InputBarcode}' tidak ditemukan!", "Tidak Ditemukan");
                    ResetInputSuggestion();
                    return;
                }

                TambahProdukKeKeranjang(produk);
                ResetInputSuggestion();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Terjadi kesalahan: {ex.Message}", "Error Scan");
            }
        }

        private void TutupSuggestion()
        {
            _isUpdatingFromSelection = true;
            DaftarSuggestionProduk.Clear();
            SelectedProdukSuggestion = null;
            IsSuggestionOpen = false;
            _isUpdatingFromSelection = false;
        }

        private void ResetInputSuggestion()
        {
            _isUpdatingFromSelection = true;
            InputBarcode = string.Empty;
            SelectedProdukSuggestion = null;
            DaftarSuggestionProduk.Clear();
            IsSuggestionOpen = false;
            _isUpdatingFromSelection = false;
        }

        private void CartItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HitungTotal();
        }

        private void TambahProdukKeKeranjang(Produk produk)
        {
            if (produk.Stok <= 0)
            {
                _dialogService.ShowWarning($"Stok produk '{produk.Nama}' habis!", "Stok Habis");
                return;
            }

            var itemInCart = Keranjang.FirstOrDefault(c => c.Produk != null && c.Produk.Id == produk.Id);
            if (itemInCart != null)
            {
                if (itemInCart.Jumlah + 1 > produk.Stok)
                {
                    _dialogService.ShowWarning($"Stok '{produk.Nama}' tidak mencukupi! Sisa stok: {produk.Stok}", "Stok Terbatas");
                    return;
                }
                itemInCart.Jumlah++;
            }
            else
            {
                var newItem = new CartItem(produk, 1);
                newItem.PropertyChanged += CartItem_PropertyChanged;
                Keranjang.Add(newItem);
            }

            HitungTotal();
        }

        [RelayCommand]
        public void HoldTransaksi()
        {
            if (Keranjang.Count == 0)
            {
                _dialogService.ShowWarning("Keranjang belanja masih kosong!", "Peringatan");
                return;
            }

            var holdItem = new HoldTransactionModel
            {
                Items = Keranjang.ToList()
            };

            DaftarHold.Add(holdItem);
            ResetKeranjang();

            _dialogService.ShowInfo($"Transaksi berhasil ditahan (ID: {holdItem.Id})!", "Hold Transaksi");
        }

        [RelayCommand]
        public void ResumeTransaksi(HoldTransactionModel? holdItem)
        {
            if (holdItem == null) return;

            if (Keranjang.Count > 0)
            {
                bool konfirmasi = _dialogService.ShowConfirmation("Keranjang saat ini tidak kosong. Apakah ingin menggabungkan item?", "Konfirmasi Resume");
                if (!konfirmasi) return;
            }

            foreach (var item in holdItem.Items)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
                item.PropertyChanged += CartItem_PropertyChanged;
                Keranjang.Add(item);
            }

            DaftarHold.Remove(holdItem);
            HitungTotal();
        }

        partial void OnTotalBayarChanged(decimal value)
        {
            HitungKembalian();
        }

        [RelayCommand]
        public void HapusItem(CartItem? item)
        {
            if (item != null)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
                Keranjang.Remove(item);
                HitungTotal();
            }
        }

        [RelayCommand]
        public void BatalTransaksi()
        {
            ResetKeranjang();
        }

        [RelayCommand]
        public async Task ProsesBayarAsync()
        {
            if (Keranjang.Count == 0)
            {
                _dialogService.ShowWarning("Keranjang belanja masih kosong!", "Peringatan");
                return;
            }

            if (TotalBayar < TotalHarga)
            {
                _dialogService.ShowWarning("Uang pembayaran kurang!", "Peringatan");
                return;
            }

            try
            {
                var listItems = Keranjang.ToList();
                decimal bayar = TotalBayar;
                int kasirId = SessionManager.CurrentKasir?.Id ?? 1;
                int? sessionId = SessionManager.CurrentSession?.Id;

                var result = await _transactionService.ProcessTransactionAsync(
                    listItems,
                    bayar,
                    kasirId,
                    sessionId
                );

                if (result.IsSuccess)
                {
                    if (result.TransaksiData != null)
                    {
                        _ = _printerService.CetakStrukAsync(result.TransaksiData);
                    }

                    _dialogService.ShowInfo(
                        $"Transaksi Berhasil!\nNota: {result.TransaksiData?.NomorNota}\nKembalian: Rp {result.Kembalian:N0}",
                        "Sukses Transaksi"
                    );

                    ResetKeranjang();
                }
                else
                {
                    _dialogService.ShowError(result.Message, "Gagal Transaksi");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Terjadi kesalahan sistem: {ex.Message}", "Error Bayar");
            }
        }

        private void HitungTotal()
        {
            TotalHarga = Keranjang.Sum(c => c.Subtotal);
            HitungKembalian();
        }

        private void HitungKembalian()
        {
            Kembalian = TotalBayar > TotalHarga ? TotalBayar - TotalHarga : 0;
        }

        private void ResetKeranjang()
        {
            foreach (var item in Keranjang)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }
            Keranjang.Clear();
            ResetInputSuggestion();
            TotalBayar = 0;
            HitungTotal();
        }

        public void Dispose()
        {
            _ctsSearch?.Dispose();
            SessionManager.SessionCleared -= OnSessionCleared;

            foreach (var item in Keranjang)
            {
                item.PropertyChanged -= CartItem_PropertyChanged;
            }
        }
    }
}