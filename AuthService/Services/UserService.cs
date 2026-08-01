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

        public async Task CreateUserAsync(User user)
        {
            // O indice unico no banco e a garantia real; esta checagem existe
            // para devolver uma mensagem clara em vez de um erro de constraint.
            if (await _userRepository.LoginExistsAsync(user.Login))
            {
                throw new InvalidOperationException("Login already in use.");
            }

            user.Password = PasswordCriptografyService.GeneratePasswordHash(user.Password);
            user.Id = Guid.NewGuid();

            await _userRepository.AddAsync(user);
        }

        public async Task<User?> AuthenticateUserAsync(string userLogin, string userPassword)
        {
            var user = await _userRepository.getLoginAsync(userLogin);

            if (user is null)
            {
                // Sem isto a resposta volta muito mais rapido quando o login nao
                // existe, permitindo descobrir quais logins estao cadastrados.
                PasswordCriptografyService.SimulateValidation(userPassword);
                return null;
            }

            if (!PasswordCriptografyService.ValidPassword(userPassword, user.Password))
            {
                return null;
            }

            // Verificado so depois da senha: quem nao sabe a senha nao descobre
            // se a conta existe e esta desativada.
            if (!user.Active)
            {
                return null;
            }

            // Unico momento em que a senha em claro esta disponivel para
            // regravar um hash antigo com o custo atual.
            if (PasswordCriptografyService.PrecisaAtualizar(user.Password))
            {
                user.Password = PasswordCriptografyService.GeneratePasswordHash(userPassword);
                await _userRepository.UpdateAsync(user);
            }

            return user;
        }

        public async Task<User?> GetUserAsync(string login)
        {
            return await _userRepository.getLoginAsync(login);
        }

    }
}
