using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public partial class KasirViewModel : ObservableObject
    {
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;

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

        public KasirViewModel()
        {
            HitungTotal();
        }

        public KasirViewModel(ITransactionService transactionService, IProductService productService)
        {
            _transactionService = transactionService;
            _productService = productService;
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
                MessageBox.Show("Keranjang belanja masih kosong!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var holdItem = new HoldTransactionModel
            {
                Items = Keranjang.ToList()
            };

            DaftarHold.Add(holdItem);

            ResetKeranjang();

            MessageBox.Show($"Transaksi berhasil ditahan (ID: {holdItem.Id})!", "Hold Transaksi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        public void ResumeTransaksi(HoldTransactionModel? holdItem)
        {
            if (holdItem == null) return;

            if (Keranjang.Count > 0)
            {
                var result = MessageBox.Show("Keranjang saat ini tidak kosong. Apakah ingin menggabungkan item?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
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
        public async Task TambahBarang()
        {
            if (string.IsNullOrWhiteSpace(InputBarcode)) return;

            var produk = await _productService.GetProductBySkuOrNameAsync(InputBarcode);

            if (produk == null)
            {
                MessageBox.Show($"Produk dengan SKU/Nama '{InputBarcode}' tidak ditemukan!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                InputBarcode = string.Empty;
                return;
            }

            if (produk.Stok <= 0)
            {
                MessageBox.Show($"Stok produk '{produk.Nama}' habis!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                InputBarcode = string.Empty;
                return;
            }

            var itemInCart = Keranjang.FirstOrDefault(c => c.Produk.Id == produk.Id);
            if (itemInCart != null)
            {
                if (itemInCart.Jumlah + 1 > produk.Stok)
                {
                    MessageBox.Show($"Stok '{produk.Nama}' tidak mencukupi! Sisa stok: {produk.Stok}", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        // Command: Simpan Transaksi & Potong Stok
        [RelayCommand]
        public async Task ProsesBayar()
        {
            if (Keranjang.Count == 0)
            {
                MessageBox.Show("Keranjang belanja masih kosong!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TotalBayar < TotalHarga)
            {
                MessageBox.Show("Uang pembayaran kurang!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                // Cetak Struk via Service
                if (result.TransaksiData != null)
                {
                    PrinterService.CetakStruk(result.TransaksiData);
                }

                MessageBox.Show(
                    $"Transaksi Berhasil!\nNota: {result.TransaksiData?.NomorNota}\nKasir: {result.TransaksiData?.NamaKasir}\nKembalian: Rp {result.Kembalian:N0}",
                    "Sukses",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                ResetKeranjang();
            }
            else
            {
                MessageBox.Show(result.Message, "Gagal Transaksi", MessageBoxButton.OK, MessageBoxImage.Error);
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