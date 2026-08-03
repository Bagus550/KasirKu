using KasirKu.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KasirKu.Services
{
    public interface IKasirService
    {
        Task<List<Kasir>> GetAllKasirAsync();
        Task<bool> TambahKasirAsync(string nama, string username, string password, string role);
        Task<bool> HapusKasirAsync(int id);
        Task<bool> UpdateKasirAsync(int id, string nama, string username, string? password, string role);
    }
}