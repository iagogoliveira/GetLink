using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class UserLoginDto
    {
        // Sem limite minimo aqui: o login so precisa existir para ser comparado
        // Exigir formato no login facilitaria distinguir "invalido" de "inexistente".
        [Required]
        [StringLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}