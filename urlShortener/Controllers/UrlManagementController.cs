using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using urlShortener.Services;

namespace urlShortener.Controllers
{
    /// <summary>
    /// Endpoints de gerenciamento. Ficam sob /api/ de proposito: a raiz e o
    /// espaco dos codigos curtos, e qualquer rota literal criada la queimaria
    /// aquele codigo para sempre.
    /// </summary>
    [ApiController]
    [Route("api/urls")]
    [Authorize]
    public class UrlManagementController : ControllerBase
    {
        private readonly ClickService _clickService;

        public UrlManagementController(ClickService clickService)
        {
            _clickService = clickService;
        }

        private Guid GetAuthenticatedUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId!);
        }

        [HttpGet]
        public async Task<IActionResult> ListarMinhasUrls()
        {
            return Ok(await _clickService.ListarUrlsDoUsuario(GetAuthenticatedUserId()));
        }

        [HttpGet("{id:guid}/stats")]
        public async Task<IActionResult> Estatisticas(Guid id)
        {
            var stats = await _clickService.ObterEstatisticas(id, GetAuthenticatedUserId());

            if (stats is null)
            {
                return NotFound("Url not found.");
            }

            return Ok(stats);
        }
    }
}
