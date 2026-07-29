using urlShortener.Models;
namespace urlShortener.Repositories

{
    public interface IUrlRepository
    {
        Task AddUrl(Address url);
        Task <Address> GetUrl(Guid url);
        Task <Address> GetUrlRedirect(string shortUrl);
        Task<Address> UpdateUrl(Address url);
        Task DeleteUrl(Guid id);
        Task<bool> ExistsAsync(string fullShortUrl);
    }
}
