using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class KasirViewModel : ObservableObject
    {
        // 1. Keranjang Belanja
        private ObservableCollection<CartItem> _keranjang = new();
        public ObservableCollection<CartItem> Keranjang
        {
            get => _keranjang;
            set => SetProperty(ref _keranjang, value);
        }

        // 2. Input Barcode / Pencarian
        private string _inputBarcode = string.Empty;
        public string InputBarcode
        {
            get => _inputBarcode;
            set => SetProperty(ref _inputBarcode, value);
        }

        // 3. Total Harga
        private decimal _totalHarga;
        public decimal TotalHarga
        {
            get => _totalHarga;
            set => SetProperty(ref _totalHarga, value);
        }

        // 4. Total Bayar (Uang dari Pembeli)
        private decimal _totalBayar;
        public decimal TotalBayar
        {
            get => _totalBayar;
            set
            {
                if (SetProperty(ref _totalBayar, value))
                {
                    HitungKembalian();
                }
            }
        }

        // 5. Uang Kembalian
        private decimal _kembalian;
        public decimal Kembalian
        {
            get => _kembalian;
            set => SetProperty(ref _kembalian, value);
        }

        public KasirViewModel()
        {
            HitungTotal();
        }

        // Command: Scan / Tambah Barang dari Input Barcode
        [RelayCommand]
        public void TambahBarang()
        {
            if (string.IsNullOrWhiteSpace(InputBarcode)) return;

            using var db = new AppDbContext();

            string keyword = InputBarcode.Trim().ToLower();

            // Pencarian fleksibel: SKU persis ATAU Nama mengandung kata kunci
            var produk = db.Produk.FirstOrDefault(p =>
                p.SKU.ToLower() == keyword ||
                p.Nama.ToLower().Contains(keyword));

            if (produk == null)
            {
                MessageBox.Show($"Produk dengan SKU/Nama '{InputBarcode}' tidak ditemukan!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                InputBarcode = string.Empty;
                return;
            }

            // Cek Stok Awal
            if (produk.Stok <= 0)
            {
                MessageBox.Show($"Stok produk '{produk.Nama}' habis!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                InputBarcode = string.Empty;
                return;
            }

            // Jika barang sudah ada di keranjang, tambah jumlahnya
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
            Keranjang.Clear();
            TotalBayar = 0;
            HitungTotal();
        }

        // Command: Simpan Transaksi & Potong Stok
        [RelayCommand]
        public void ProsesBayar()
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

            try
            {
                using var db = new AppDbContext();

                // 1. Buat Header Transaksi
                var transaksi = new Transaksi
                {
                    NomorNota = $"INV/{DateTime.Now:yyyyMMdd}/{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    Tanggal = DateTime.Now,
                    TotalHarga = TotalHarga,
                    TotalBayar = TotalBayar,
                    Kembalian = Kembalian
                };

                // 2. Buat Detail Transaksi & Potong Stok Produk
                foreach (var cartItem in Keranjang)
                {
                    // Ambil entity produk langsung dari DB agar tracking EF Core valid
                    var produkDb = db.Produk.Find(cartItem.Produk.Id);
                    if (produkDb != null)
                    {
                        produkDb.Stok -= cartItem.Jumlah;
                    }

                    var detail = new DetailTransaksi
                    {
                        ProdukId = cartItem.Produk.Id,
                        NamaProduk = cartItem.Produk.Nama,
                        HargaJual = cartItem.HargaJual,
                        Jumlah = cartItem.Jumlah
                    };

                    transaksi.DetailTransaksi.Add(detail);
                }

                db.Transaksi.Add(transaksi);
                db.SaveChanges();

                Services.PrinterService.CetakStruk(transaksi);

                MessageBox.Show($"Transaksi Berhasil!\nNota: {transaksi.NomorNota}\nKembalian: Rp {Kembalian:N0}", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear Keranjang setelah sukses
                BatalTransaksi();
            }
            catch (Exception ex)
            {
                // Tangkap pesan inner exception asli dari SQLite jika ada error
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Gagal menyimpan transaksi: {innerMsg}", "Error Database", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}