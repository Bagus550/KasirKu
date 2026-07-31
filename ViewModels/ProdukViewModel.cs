using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KasirKu.ViewModels
{
    public partial class ProdukViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<Produk> _daftarProduk = new();

        [ObservableProperty]
        private Produk _selectedProduk = new();

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        // Constructor Utama: Menerima IDialogService dari DI Container
        public ProdukViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            MuatDataProduk();
        }

        [RelayCommand]
        public void MuatDataProduk()
        {
            try
            {
                using var db = new AppDbContext();
                db.Database.EnsureCreated();

                var query = db.Produk.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(p => p.Nama.ToLower().Contains(keyword) ||
                                             (p.SKU != null && p.SKU.ToLower().Contains(keyword)));
                }

                DaftarProduk = new ObservableCollection<Produk>(query.ToList());
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat data produk: {ex.Message}", "Error Database");
            }
        }

        [RelayCommand]
        public void SimpanProduk()
        {
            if (SelectedProduk == null)
                return;

            if (string.IsNullOrWhiteSpace(SelectedProduk.Nama))
            {
                _dialogService.ShowWarning("Nama produk wajib diisi!", "Peringatan");
                return;
            }

            if (SelectedProduk.HargaJual <= 0)
            {
                _dialogService.ShowWarning("Harga jual harus lebih besar dari 0!", "Peringatan");
                return;
            }

            try
            {
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

                _dialogService.ShowInfo("Data produk berhasil disimpan!", "Sukses");
                ResetForm();
                MuatDataProduk();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal menyimpan produk: {ex.Message}", "Error Simpan");
            }
        }

        [RelayCommand]
        public void HapusProduk(Produk? produk)
        {
            if (produk == null)
                return;

            bool confirm = _dialogService.ShowConfirmation(
                $"Apakah Anda yakin ingin menghapus produk '{produk.Nama}'?",
                "Konfirmasi Hapus"
            );

            if (!confirm)
                return;

            try
            {
                using var db = new AppDbContext();
                db.Produk.Remove(produk);
                db.SaveChanges();

                _dialogService.ShowInfo("Produk berhasil dihapus!", "Sukses");
                MuatDataProduk();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal menghapus produk: {ex.Message}", "Error Hapus");
            }
        }

        [RelayCommand]
        public void ResetForm()
        {
            SelectedProduk = new Produk();
        }
    }
}