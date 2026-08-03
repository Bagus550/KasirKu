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
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ProductService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Produk>> GetAllProductsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Produk.ToListAsync();
        }

        public async Task<Produk?> GetProductByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Produk.FindAsync(id);
        }

        public async Task<Produk?> GetProductBySkuOrNameAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            string searchKeyword = query.Trim();
            string pattern = $"%{searchKeyword}%";

            using var context = await _contextFactory.CreateDbContextAsync();

            // 1. Prioritaskan pencarian SKU yang sama persis
            var produkExact = await context.Produk.FirstOrDefaultAsync(p =>
                p.SKU != null && p.SKU.ToLower() == searchKeyword.ToLower());

            if (produkExact != null) return produkExact;

            // 2. Jika tidak ada SKU pas, cari produk berdasarkan kata kunci sebagian
            return await context.Produk.FirstOrDefaultAsync(p =>
                (p.SKU != null && EF.Functions.Like(p.SKU, pattern)) ||
                (p.Nama != null && EF.Functions.Like(p.Nama, pattern)));
        }

        public async Task<List<Produk>> SearchProductsAsync(string query, int limit = 10)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(query))
            {
                return await context.Produk.Take(limit).ToListAsync();
            }

            string pattern = $"%{query.Trim()}%";

            return await context.Produk
                .Where(p => (p.Nama != null && EF.Functions.Like(p.Nama, pattern)) ||
                            (p.SKU != null && EF.Functions.Like(p.SKU, pattern)))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> IsStockAvailableAsync(int productId, int quantity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var produk = await context.Produk.FindAsync(productId);
            return produk != null && produk.Stok >= quantity;
        }

        public async Task<bool> AddProductAsync(Produk produk)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Produk.Add(produk);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProductAsync(Produk produk)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Produk.Update(produk);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var produk = await context.Produk.FindAsync(id);
            if (produk == null) return false;

            context.Produk.Remove(produk);
            return await context.SaveChangesAsync() > 0;
        }
    }
}