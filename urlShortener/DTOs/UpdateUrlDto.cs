using System.ComponentModel.DataAnnotations;

namespace urlShortener.DTOs
{
    public class UpdateUrlDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(2048)]
        public string OriginalUrl { get; set; } = string.Empty;

        // Opcional: nulo ou vazio mantem o caminho atual.
        [StringLength(100)]
        public string? NewPath { get; set; }
    }
}
