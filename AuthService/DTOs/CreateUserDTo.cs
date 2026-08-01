using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class CreateUserDto
    {
        // Tamanhos alinhados com o mapeamento em AppDbContext, para a validacao
        // barrar antes de o banco reclamar.
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter ao menos 8 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;
    }
}