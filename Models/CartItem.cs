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
                if (value < 1)
                {
                    value = 1;
                }

                if (value > Produk.Stok)
                    value = Produk.Stok;

                if (_jumlah == value)
                    return;

                _jumlah = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));

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