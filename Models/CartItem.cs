using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace KasirKu.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        public Produk Produk { get; set; } = new();

        private int _jumlah = 1;
        public int Jumlah
        {
            get => _jumlah;
            set
            {
                // Validasi 1: Jumlah tidak boleh kurang dari 1
                if (value < 1)
                {
                    MessageBox.Show("Jumlah barang minimal 1!", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    OnPropertyChanged(); // Trigger update UI agar kembali ke nilai lama
                    return;
                }

                // Validasi 2: Jumlah tidak boleh melebihi stok yang ada
                if (Produk != null && value > Produk.Stok)
                {
                    MessageBox.Show($"Stok '{Produk.Nama}' tidak mencukupi! Sisa stok: {Produk.Stok}", "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _jumlah = Produk.Stok; // Set ke stok maksimal
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                    return;
                }

                if (_jumlah != value)
                {
                    _jumlah = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal HargaJual => Produk.HargaJual;
        public decimal Subtotal => HargaJual * Jumlah;

        public CartItem(Produk produk, int jumlah = 1)
        {
            Produk = produk;
            Jumlah = jumlah;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}