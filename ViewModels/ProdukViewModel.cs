using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasirKu.Data;
using KasirKu.Models;
using KasirKu.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KasirKu.ViewModels
{
    public partial class ProdukViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<Produk> _daftarProduk = new();

        [ObservableProperty]
        private Produk _selectedProduk = new();

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        public ProdukViewModel(IDbContextFactory<AppDbContext> contextFactory, IDialogService dialogService)
        {
            _contextFactory = contextFactory;
            _dialogService = dialogService;
            _ = MuatDataProdukAsync();
        }

        [RelayCommand]
        public async Task MuatDataProdukAsync()
        {
            try
            {
                using var db = await _contextFactory.CreateDbContextAsync();

                var query = db.Produk.AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(p => p.Nama.ToLower().Contains(keyword) ||
                                             (p.SKU != null && p.SKU.ToLower().Contains(keyword)));
                }

                var list = await query.ToListAsync();
                DaftarProduk = new ObservableCollection<Produk>(list);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal memuat data produk: {ex.Message}", "Error Database");
            }
        }

        [RelayCommand]
        public async Task SimpanProdukAsync()
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
                using var db = await _contextFactory.CreateDbContextAsync();
                int currentKasirId = SessionManager.CurrentKasir?.Id ?? 0;

                if (SelectedProduk.Id == 0)
                {
                    db.Produk.Add(SelectedProduk);

                    db.AuditLog.Add(new AuditLog
                    {
                        KasirId = currentKasirId,
                        Waktu = DateTime.Now,
                        JenisAksi = "TAMBAH_PRODUK",
                        Keterangan = $"Menambah produk baru '{SelectedProduk.Nama}' (SKU: {SelectedProduk.SKU ?? "-"}), " +
                                     $"Harga Jual: Rp{SelectedProduk.HargaJual:N0}, Stok Awal: {SelectedProduk.Stok}"
                    });
                }
                else
                {
                    var produkDb = await db.Produk.FindAsync(SelectedProduk.Id);

                    if (produkDb != null)
                    {
                        string infoPerubahan = $"Mengubah produk '{produkDb.Nama}' (ID: {produkDb.Id}). " +
                                               $"Harga Jual: Rp{produkDb.HargaJual:N0} -> Rp{SelectedProduk.HargaJual:N0}, " +
                                               $"Stok: {produkDb.Stok} -> {SelectedProduk.Stok}";

                        produkDb.SKU = SelectedProduk.SKU;
                        produkDb.Nama = SelectedProduk.Nama;
                        produkDb.Kategori = SelectedProduk.Kategori;
                        produkDb.HargaBeli = SelectedProduk.HargaBeli;
                        produkDb.HargaJual = SelectedProduk.HargaJual;
                        produkDb.Stok = SelectedProduk.Stok;
                        produkDb.StokMinimum = SelectedProduk.StokMinimum;

                        db.AuditLog.Add(new AuditLog
                        {
                            KasirId = currentKasirId,
                            Waktu = DateTime.Now,
                            JenisAksi = "EDIT_PRODUK",
                            Keterangan = infoPerubahan
                        });
                    }
                    else
                    {
                        _dialogService.ShowError("Data produk tidak ditemukan di database!", "Error Simpan");
                        return;
                    }
                }

                await db.SaveChangesAsync();

                _dialogService.ShowInfo("Data produk berhasil disimpan!", "Sukses");
                ResetForm();
                await MuatDataProdukAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Gagal menyimpan produk: {ex.Message}", "Error Simpan");
            }
        }

        [RelayCommand]
        public async Task HapusProdukAsync(Produk? produk)
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
                using var db = await _contextFactory.CreateDbContextAsync();

                var produkDb = await db.Produk.FindAsync(produk.Id);
                if (produkDb != null)
                {
                    db.Produk.Remove(produkDb);

                    db.AuditLog.Add(new AuditLog
                    {
                        KasirId = SessionManager.CurrentKasir?.Id ?? 0,
                        Waktu = DateTime.Now,
                        JenisAksi = "HAPUS_PRODUK",
                        Keterangan = $"Menghapus produk '{produkDb.Nama}' (SKU: {produkDb.SKU ?? "-"}, Stok Akhir: {produkDb.Stok}) dari sistem."
                    });

                    await db.SaveChangesAsync();

                    _dialogService.ShowInfo("Produk berhasil dihapus!", "Sukses");
                    await MuatDataProdukAsync();
                }
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