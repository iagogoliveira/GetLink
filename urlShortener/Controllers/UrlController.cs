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
        public UrlController(UrlService urlService, RequestHandlerService requestHandlerService) 
        { 
            _urlService = urlService;
            _requestHandlerService = requestHandlerService;
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

            return Redirect(urlRedirect.OriginalUrl);
        }
    }
}
