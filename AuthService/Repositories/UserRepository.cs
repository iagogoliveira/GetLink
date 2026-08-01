using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;


namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) { _context = context; }



        public async Task AddAsync(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
        }



        public async Task UpdateAsync(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }



        public async Task<User?> getLoginAsync(string login)
        {
            return await _context.Set<User>()
                                 .FirstOrDefaultAsync(u => u.Login == login);
        }



        public async Task<bool> LoginExistsAsync(string login)
        {
            return await _context.Set<User>()
                                 .AnyAsync(u => u.Login == login);
        }
    }
}
