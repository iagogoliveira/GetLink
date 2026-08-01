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


        public async Task<Address?> GetUrl(Guid url)
        {
            return await _context.Addresses.FindAsync(url);
        }


        public async Task<Address?> GetUrlRedirect(string shortUrl)
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


        public async Task AddClick(Click click)
        {
            _context.Clicks.Add(click);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Address>> GetUrlsByUser(Guid userId)
        {
            return await _context.Addresses
                                 .AsNoTracking()
                                 .Where(a => a.UserId == userId)
                                 .OrderByDescending(a => a.CreatedAt)
                                 .ToListAsync();
        }


        public async Task<List<ClickAggregate>> GetClickAggregatesByUser(Guid userId)
        {
            // Agregado no banco: trazer uma linha por clique so para conta-las
            // ficaria caro conforme as URLs acumulam acessos.
            return await _context.Clicks
                                 .AsNoTracking()
                                 .Where(c => c.Address!.UserId == userId)
                                 .GroupBy(c => c.AddressId)
                                 .Select(g => new ClickAggregate
                                 {
                                     AddressId = g.Key,
                                     Total = g.Count(),
                                     UltimoClique = g.Max(c => c.ClickedAt)
                                 })
                                 .ToListAsync();
        }


        public async Task<List<Click>> GetClicks(Guid addressId)
        {
            return await _context.Clicks
                                 .AsNoTracking()
                                 .Where(c => c.AddressId == addressId)
                                 .ToListAsync();
        }

        public async Task DeleteUrl(Guid id)
        {
            var url = await GetUrl(id);

            // Idempotente: apagar o que ja nao existe nao e erro. Quem precisa
            // responder 404 e o servico, que verifica antes por causa do dono.
            if (url is null)
            {
                return;
            }

            _context.Addresses.Remove(url);
            await _context.SaveChangesAsync();
        }

    }
}
