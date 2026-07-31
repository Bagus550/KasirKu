using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public class HoldTransactionModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString()[..5].ToUpper();
        public DateTime Waktu { get; set; } = DateTime.Now;
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalHarga => Items.Sum(x => x.Subtotal);
    }

    public partial class KasirViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly IPrinterService _printerService;

        // Properties UI menggunakan CommunityToolkit Source Generator
        [ObservableProperty]
        private ObservableCollection<CartItem> _keranjang = new();

        [ObservableProperty]
        private ObservableCollection<HoldTransactionModel> _daftarHold = new();

        [ObservableProperty]
        private string _inputBarcode = string.Empty;

        [ObservableProperty]
        private decimal _totalHarga;

        [ObservableProperty]
        private decimal _totalBayar;

        [ObservableProperty]
        private decimal _kembalian;

        // Constructor Utama (Semua dependensi di-inject via DI Container)
        public KasirViewModel(
            ITransactionService transactionService,
            IProductService productService,
            IDialogService dialogService,
            IPrinterService printerService)
        {
            _transactionService = transactionService;
            _productService = productService;
            _dialogService = dialogService;
            _printerService = printerService;

            HitungTotal();
        }

        partial void OnTotalBayarChanged(decimal value)
        {
            HitungKembalian();
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
                item.PropertyChanged += (s, e) => HitungTotal();
                Keranjang.Add(item);
            }

            DaftarHold.Remove(holdItem);
            HitungTotal();
        }

        // Command: Scan / Tambah Barang dari Input Barcode
        [RelayCommand]
        public async Task TambahBarangAsync()
        {
            if (string.IsNullOrWhiteSpace(InputBarcode)) return;

            try
            {
                var produk = await _productService.GetProductBySkuOrNameAsync(InputBarcode);

                if (produk == null)
                {
                    _dialogService.ShowWarning($"Produk dengan SKU/Nama '{InputBarcode}' tidak ditemukan!", "Tidak Ditemukan");
                    InputBarcode = string.Empty;
                    return;
                }

                if (produk.Stok <= 0)
                {
                    _dialogService.ShowWarning($"Stok produk '{produk.Nama}' habis!", "Stok Habis");
                    InputBarcode = string.Empty;
                    return;
                }

                var itemInCart = Keranjang.FirstOrDefault(c => c.Produk != null && c.Produk.Id == produk.Id);
                if (itemInCart != null)
                {
                    if (itemInCart.Jumlah + 1 > produk.Stok)
                    {
                        _dialogService.ShowWarning($"Stok '{produk.Nama}' tidak mencukupi! Sisa stok: {produk.Stok}", "Stok Terbatas");
                        InputBarcode = string.Empty;
                        return;
                    }
                    itemInCart.Jumlah++;
                }
                else
                {
                    var newItem = new CartItem(produk, 1);
                    newItem.PropertyChanged += (s, e) => HitungTotal();
                    Keranjang.Add(newItem);
                }

                InputBarcode = string.Empty;
                HitungTotal();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Terjadi kesalahan: {ex.Message}", "Error Scan");
            }
        }

        // Command: Hapus Item dari Keranjang
        [RelayCommand]
        public void HapusItem(CartItem? item)
        {
            if (item != null)
            {
                Keranjang.Remove(item);
                HitungTotal();
            }
        }

        // Command: Reset / Batalkan Seluruh Transaksi
        [RelayCommand]
        public void BatalTransaksi()
        {
            ResetKeranjang();
        }

        // Command: Simpan Transaksi, Potong Stok & Cetak Struk Async
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

            var result = await _transactionService.ProcessTransactionAsync(
                Keranjang.ToList(),
                TotalBayar,
                SessionManager.CurrentKasir?.Id ?? 1,
                SessionManager.CurrentSession?.Id
            );

            if (result.IsSuccess)
            {
                // Cetak Struk secara Async via PrinterService
                if (result.TransaksiData != null)
                {
                    await _printerService.CetakStrukAsync(result.TransaksiData);
                }

                _dialogService.ShowInfo(
                    $"Transaksi Berhasil!\nNota: {result.TransaksiData?.NomorNota}\nKasir: {result.TransaksiData?.NamaKasir}\nKembalian: Rp {result.Kembalian:N0}",
                    "Sukses Transaksi"
                );

                ResetKeranjang();
            }
            else
            {
                _dialogService.ShowError(result.Message, "Gagal Transaksi");
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
            Keranjang.Clear();
            TotalBayar = 0;
            HitungTotal();
        }
    }
}