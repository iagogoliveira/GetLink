using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userServices;
        private readonly TokenService _tokenService;
        public AuthController(UserService userService, TokenService tokenService)
        {
            _userServices = userService;
            _tokenService = tokenService;
        }

        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] CreateUserDto userDto)   
        {

            if (userDto == null)
            {
                return BadRequest("User cannot be null.");
            }


            var user = new User(userDto.Name, userDto.Login, userDto.Password, userDto.Email);

            try
            {
                _userServices.CreateUser(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            return Ok();

        }

        [HttpPost("Login")]
        public async Task<IActionResult> UserLogin([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userLogin = await _userServices.AuthenticateUserAsync(loginDto.Login, loginDto.Password);

            if (userLogin != null)
            {
                var token = await _tokenService.GenerateTokenAsync(userLogin.Id.ToString(), userLogin.Email);
                return CreatedAtAction(nameof(UserLogin), new { Token = token });
            }
            else
            {
                return Unauthorized(new { message = "Invalid Credentials." });
            }
        }
    }
}
