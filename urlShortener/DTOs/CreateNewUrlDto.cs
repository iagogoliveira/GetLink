using System.ComponentModel.DataAnnotations;

namespace urlShortener.DTOs
{
    public class CreateNewUrlDto
    {
        // O dono da URL vem do claim do token, nunca do corpo da requisicao.
        [Required]
        [StringLength(2048)]
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
