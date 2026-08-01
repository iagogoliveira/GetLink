using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using urlShortener.DTOs;
using urlShortener.Models;
using urlShortener.Services;

namespace urlShortener.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {

        private readonly UrlService _urlService;
        private readonly RequestHandlerService _requestHandlerService;
        private readonly ClickService _clickService;
        private readonly ILogger<UrlController> _logger;

        public UrlController(
            UrlService urlService,
            RequestHandlerService requestHandlerService,
            ClickService clickService,
            ILogger<UrlController> logger)
        {
            _urlService = urlService;
            _requestHandlerService = requestHandlerService;
            _clickService = clickService;
            _logger = logger;
        }

        // O id do usuario vem sempre do token, nunca do corpo da requisicao.
        private Guid GetAuthenticatedUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userId!);
        }

        [Authorize]
        [HttpPost("CreateNewUrl")]
        public async Task<IActionResult> CreateNewUrl([FromBody] CreateNewUrlDto urlDto)
        {
            var url = new Address(
                Guid.NewGuid(),
                urlDto.OriginalUrl,
                GetAuthenticatedUserId()
            );

            return await _requestHandlerService.HandleRequest(async () =>
            {
                var createdUrl = await _urlService.CreateNewUrl(url);
                return new CreateNewUrlResponseDto { NewUrl = createdUrl.NewUrl };
            });
        }

        [Authorize]
        [HttpPut("UpdateUrl")]
        public async Task<IActionResult> UpdateUrl([FromBody] UpdateUrlDto urlDto)
        {
            var userId = GetAuthenticatedUserId();

            return await _requestHandlerService.HandleRequest(() => _urlService.UpdateUrl(urlDto.Id, userId, urlDto.OriginalUrl, urlDto.NewPath));
        }

        [Authorize]
        [HttpDelete("DeleteUrl")]
        public async Task<IActionResult> DeleteUrl([FromBody] DeleteUrlDto urlDto)
        {
            var userId = GetAuthenticatedUserId();

            return await _requestHandlerService.HandleRequest(() => _urlService.DeleteUrl(urlDto.Id, userId));
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectUrl(string code)
        {
            var urlRedirect = await _urlService.GetUrlRedirect(code);

            if (urlRedirect is null)
            {
                return NotFound("Url not found.");
            }

            // Falha na estatistica nao pode custar o redirect ao usuario, que e a
            // funcao principal do produto. Registra o erro e segue.
            try
            {
                await _clickService.RegistrarClique(
                    urlRedirect.Id,
                    Request.Headers.UserAgent.ToString(),
                    Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao registrar clique da url {UrlId}.", urlRedirect.Id);
            }

            return Redirect(urlRedirect.OriginalUrl);
        }
    }
}
