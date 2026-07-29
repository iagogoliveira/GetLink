using AuthService.Models;
using AuthService.Repositories;

namespace AuthService.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository usuarioRepository)
        {
            _userRepository = usuarioRepository;
        }

        public void CreateUser(User user)
        {
            user.Password = PasswordCriptografyService.GeneratePasswordHash(user.Password);
            user.Id = Guid.NewGuid();

            _userRepository.Add(user);
        }

        public async Task<User?> AuthenticateUserAsync(string userLogin, string userPassword)
        {
            var user = await _userRepository.getLoginAsync(userLogin);

            if (user != null && PasswordCriptografyService.ValidPassword(userPassword, user.Password))
            {
                return user;
            }

            return null;
        }

        public User GetUser(string login)
        {
            return _userRepository.getLoginAsync(login).Result;
        }

    }
}
