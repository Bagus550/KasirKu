using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KasirKu.Data;
using KasirKu.Models;

namespace KasirKu.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Produk?> GetProductBySkuOrNameAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return null;

            string cleanKeyword = keyword.Trim().ToLower();

            // Query asinkron ke SQLite via EF Core
            return await _context.Produk
                .AsNoTracking() // AsNoTracking dipakai agar query lebih cepat karena hanya membaca data
                .FirstOrDefaultAsync(p =>
                    p.SKU.ToLower() == cleanKeyword ||
                    p.Nama.ToLower().Contains(cleanKeyword));
        }

        public async Task<List<Produk>> GetAllProductsAsync()
        {
            return await _context.Produk
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsStockAvailableAsync(int produkId, int requestedQuantity)
        {
            var produk = await _context.Produk
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == produkId);

            if (produk == null) return false;

            return produk.Stok >= requestedQuantity;
        }
    }
}