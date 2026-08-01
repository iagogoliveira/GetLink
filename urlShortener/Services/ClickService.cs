using urlShortener.DTOs;
using urlShortener.Models;
using urlShortener.Repositories;

namespace urlShortener.Services
{
    public class ClickService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly ClickMetadataService _metadataService;

        public ClickService(IUrlRepository urlRepository, ClickMetadataService metadataService)
        {
            _urlRepository = urlRepository;
            _metadataService = metadataService;
        }

        /// <summary>
        /// Registra o acesso ja convertido em metadados derivados. O user-agent e
        /// o referer chegam aqui e nao sao persistidos como vieram.
        /// </summary>
        public async Task RegistrarClique(Guid addressId, string? userAgent, string? referer)
        {
            var (dispositivo, navegador, sistema) = _metadataService.Derivar(userAgent);

            await _urlRepository.AddClick(new Click
            {
                Id = Guid.NewGuid(),
                AddressId = addressId,
                ClickedAt = DateTime.UtcNow,
                RefererHost = _metadataService.ExtrairRefererHost(referer),
                DeviceType = dispositivo,
                Browser = navegador,
                OperatingSystem = sistema
            });
        }

        public async Task<List<UrlSummaryDto>> ListarUrlsDoUsuario(Guid userId)
        {
            var urls = await _urlRepository.GetUrlsByUser(userId);
            var agregados = await _urlRepository.GetClickAggregatesByUser(userId);

            var porUrl = agregados.ToDictionary(a => a.AddressId);

            return urls.Select(url =>
            {
                porUrl.TryGetValue(url.Id, out var totais);

                return new UrlSummaryDto
                {
                    Id = url.Id,
                    OriginalUrl = url.OriginalUrl,
                    ShortUrl = url.NewUrl,
                    CreatedAt = url.CreatedAt,
                    TotalClicks = totais?.Total ?? 0,
                    LastClickedAt = totais?.UltimoClique
                };
            }).ToList();
        }

        /// <summary>
        /// Estatisticas de uma URL. Retorna null quando ela nao existe ou nao
        /// pertence ao usuario -- os dois casos respondem igual, para nao revelar
        /// quais ids existem.
        /// </summary>
        public async Task<UrlStatsDto?> ObterEstatisticas(Guid addressId, Guid userId)
        {
            var url = await _urlRepository.GetUrl(addressId);

            if (url is null || url.UserId != userId)
            {
                return null;
            }

            var cliques = await _urlRepository.GetClicks(addressId);

            return new UrlStatsDto
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortUrl = url.NewUrl,
                CreatedAt = url.CreatedAt,
                TotalClicks = cliques.Count,
                LastClickedAt = cliques.Count == 0 ? null : cliques.Max(c => c.ClickedAt),

                ClicksPorDia = cliques
                    .GroupBy(c => DateOnly.FromDateTime(c.ClickedAt))
                    .OrderBy(g => g.Key)
                    .Select(g => new ClicksPorDiaDto { Dia = g.Key, Total = g.Count() })
                    .ToList(),

                PorNavegador = Agrupar(cliques, c => c.Browser),
                PorDispositivo = Agrupar(cliques, c => c.DeviceType),
                PorSistema = Agrupar(cliques, c => c.OperatingSystem),

                // Acesso direto (sem referer) e uma categoria legitima do relatorio,
                // nao um dado faltante.
                PorOrigem = Agrupar(cliques, c => c.RefererHost ?? "Acesso direto")
            };
        }

        private static List<ContagemPorChaveDto> Agrupar(
            List<Click> cliques, Func<Click, string> chave)
        {
            return cliques
                .GroupBy(chave)
                .OrderByDescending(g => g.Count())
                .Select(g => new ContagemPorChaveDto { Chave = g.Key, Total = g.Count() })
                .ToList();
        }
    }
}
