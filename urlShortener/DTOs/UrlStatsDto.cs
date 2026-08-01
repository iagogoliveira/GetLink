namespace urlShortener.DTOs
{
    public class UrlStatsDto
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalClicks { get; set; }
        public DateTime? LastClickedAt { get; set; }

        public List<ClicksPorDiaDto> ClicksPorDia { get; set; } = [];
        public List<ContagemPorChaveDto> PorNavegador { get; set; } = [];
        public List<ContagemPorChaveDto> PorDispositivo { get; set; } = [];
        public List<ContagemPorChaveDto> PorSistema { get; set; } = [];
        public List<ContagemPorChaveDto> PorOrigem { get; set; } = [];
    }

    public class ClicksPorDiaDto
    {
        public DateOnly Dia { get; set; }
        public int Total { get; set; }
    }

    public class ContagemPorChaveDto
    {
        public string Chave { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
