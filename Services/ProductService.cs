using System;
using System.Collections.Generic;
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

            string cleanKeyword = keyword.Trim();

            return await _context.Produk
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    EF.Functions.Like(p.SKU, cleanKeyword) ||
                    EF.Functions.Like(p.Nama, $"%{cleanKeyword}%"));
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