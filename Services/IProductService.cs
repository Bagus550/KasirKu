using System.Collections.Generic;
using System.Threading.Tasks;
using KasirKu.Models;

namespace KasirKu.Services
{
    public interface IProductService
    {
        /// <summary>
        /// Mencari produk berdasarkan SKU/Barcode (persis) atau Nama Produk (contain).
        /// </summary>
        Task<Produk?> GetProductBySkuOrNameAsync(string keyword);

        /// <summary>
        /// Mengambil seluruh daftar produk (opsional untuk halaman manajemen produk).
        /// </summary>
        Task<List<Produk>> GetAllProductsAsync();

        /// <summary>
        /// Memeriksa ketersediaan stok produk.
        /// </summary>
        Task<bool> IsStockAvailableAsync(int produkId, int requestedQuantity);
    }
}