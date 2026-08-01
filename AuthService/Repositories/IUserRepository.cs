using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task<User?> getLoginAsync(string login);

        Task<bool> LoginExistsAsync(string login);
    }
}
