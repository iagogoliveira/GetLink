using Microsoft.EntityFrameworkCore;
using urlShortener.Data;
using urlShortener.Models;

namespace urlShortener.Repositories
{
    public class UrlRepository : IUrlRepository
    {
        private readonly AppDbContext _context;

        public UrlRepository(AppDbContext context) 
        {
            _context = context;
        }


        public async Task AddUrl(Address url)
        {
            _context.Add(url);
            await _context.SaveChangesAsync();
        }


        public async Task<Address> GetUrl(Guid url)
        {
            return await _context.Addresses.FindAsync(url);
        }
        
        
        public async Task<Address> GetUrlRedirect(string shortUrl)
        {
            return await _context.Addresses.FirstOrDefaultAsync(_ => _.NewUrl == shortUrl);
        }

        public async Task<Address> UpdateUrl(Address url)
        {
            _context.Update(url);
            await _context.SaveChangesAsync();
            return url;
        }

        public async Task<bool> ExistsAsync(string fullShortUrl)
        {
            return await _context.Addresses.AnyAsync(u => u.NewUrl == fullShortUrl);
        }

        public async Task DeleteUrl(Guid id)
        {
            _context.Addresses.Remove(await GetUrl(id));
            await _context.SaveChangesAsync();
        }

    }
}
