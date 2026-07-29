using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        void Add(User user);

        Task<User> getLoginAsync(string login);
    }
}
