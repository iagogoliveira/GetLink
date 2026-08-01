using urlShortener.Models;
namespace urlShortener.Repositories

{
    public interface IUrlRepository
    {
        Task AddUrl(Address url);
        // Anulaveis de propósito: id ou codigo inexistente devolve null, e quem
        // chama precisa tratar isso.
        Task<Address?> GetUrl(Guid url);
        Task<Address?> GetUrlRedirect(string shortUrl);
        Task<Address> UpdateUrl(Address url);
        Task DeleteUrl(Guid id);
        Task<bool> ExistsAsync(string fullShortUrl);

        Task AddClick(Click click);
        Task<List<Address>> GetUrlsByUser(Guid userId);
        Task<List<ClickAggregate>> GetClickAggregatesByUser(Guid userId);
        Task<List<Click>> GetClicks(Guid addressId);
    }
}
