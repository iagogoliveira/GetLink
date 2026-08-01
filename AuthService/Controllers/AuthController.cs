using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userServices;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, TokenService tokenService, ILogger<AuthController> logger)
        {
            _userServices = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        // [ApiController] ja devolve 400 automaticamente quando o DTO e nulo ou
        // viola as DataAnnotations, entao nao ha checagem manual de ModelState aqui.
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
        {
            var user = new User(userDto.Name, userDto.Login, userDto.Password, userDto.Email);

            try
            {
                await _userServices.CreateUserAsync(user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cadastro recusado para o login {Login}: {Motivo}", userDto.Login, ex.Message);
                return BadRequest(ex.Message);
            }

            _logger.LogInformation("Usuario {UserId} cadastrado.", user.Id);

            return Ok();
        }

        [HttpPost("Login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> UserLogin([FromBody] UserLoginDto loginDto)
        {
            var userLogin = await _userServices.AuthenticateUserAsync(loginDto.Login, loginDto.Password);

            if (userLogin is null)
            {
                // Sem distinguir login inexistente de senha errada, nem no log.
                _logger.LogWarning("Falha de autenticacao para o login {Login}.", loginDto.Login);
                return Unauthorized(new { message = "Invalid Credentials." });
            }

            var token = await _tokenService.GenerateTokenAsync(userLogin.Id.ToString(), userLogin.Email);

            _logger.LogInformation("Usuario {UserId} autenticado.", userLogin.Id);

            // 200: autenticar nao cria recurso nenhum.
            return Ok(new { Token = token });
        }
    }
}