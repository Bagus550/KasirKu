using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace KasirKu.ViewModels
{
    public partial class ProdukViewModel : ObservableObject
    {
        private ObservableCollection<Produk> _daftarProduk = new();
        public ObservableCollection<Produk> DaftarProduk
        {
            get => _daftarProduk;
            set => SetProperty(ref _daftarProduk, value);
        }

        private Produk _selectedProduk = new();
        public Produk SelectedProduk
        {
            get => _selectedProduk;
            set => SetProperty(ref _selectedProduk, value);
        }

        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        public ProdukViewModel()
        {
            // Mencegah query database saat di XAML Designer
            bool isInDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!isInDesignMode)
            {
                MuatDataProduk();
            }
        }

        [RelayCommand]
        public void MuatDataProduk()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            var query = db.Produk.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                query = query.Where(p => p.Nama.ToLower().Contains(SearchKeyword.ToLower()) ||
                                         (p.SKU != null && p.SKU.ToLower().Contains(SearchKeyword.ToLower())));
            }

            DaftarProduk = new ObservableCollection<Produk>(query.ToList());
        }

        [RelayCommand]
        public void SimpanProduk()
        {
            if (SelectedProduk == null || string.IsNullOrWhiteSpace(SelectedProduk.Nama))
                return;

            using var db = new AppDbContext();

            if (SelectedProduk.Id == 0)
            {
                db.Produk.Add(SelectedProduk);
            }
            else
            {
                db.Produk.Update(SelectedProduk);
            }

            db.SaveChanges();
            ResetForm();
            MuatDataProduk();
        }

        [RelayCommand]
        public void HapusProduk(Produk? produk)
        {
            if (produk == null) return;

            using var db = new AppDbContext();
            db.Produk.Remove(produk);
            db.SaveChanges();

            MuatDataProduk();
        }

        [RelayCommand]
        public void ResetForm()
        {
            SelectedProduk = new Produk();
        }
    }
}