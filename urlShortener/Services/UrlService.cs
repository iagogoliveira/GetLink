using urlShortener.Repositories;
using urlShortener.Models;
using System;
using System.Text.RegularExpressions;

namespace urlShortener.Services
{
    public class UrlService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly UrlGeneratorService _urlGeneratorService;
        private readonly UrlValidatorService _urlValidatorService;

        public UrlService(IUrlRepository urlRepository, UrlValidatorService urlValidatorService, UrlGeneratorService urlGeneratorService)
        {
            _urlRepository = urlRepository;
            _urlValidatorService = urlValidatorService;
            _urlGeneratorService = urlGeneratorService;
        }
        public async Task<Address> CreateNewUrl(Address url)
        {
            if (string.IsNullOrEmpty(url.OriginalUrl))
            {
                throw new InvalidOperationException("Url cannot be null.");
            }

            if (!_urlValidatorService.CheckValidUrl(url.OriginalUrl))
            {
                throw new InvalidOperationException("Invalid URL.");
            }

            url.OriginalUrl = _urlValidatorService.NormalizeUrl(url.OriginalUrl);

            try
            {
                url.NewUrl = await _urlGeneratorService.GenerateFullUrl();
                await _urlRepository.AddUrl(url);

                return url;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task UpdateUrl(Guid id, Guid userId, string originalUrl, string? path)
        {

            if (string.IsNullOrEmpty(originalUrl))
            {
                throw new InvalidOperationException("Url cannot be null.");
            }

            if (!_urlValidatorService.CheckValidUrl(originalUrl))
            {
                throw new InvalidOperationException("Invalid URL.");
            }

            originalUrl = _urlValidatorService.NormalizeUrl(originalUrl);

            var urlObject = await GetUrl(id);

            // Url de outro usuario responde igual a inexistente, para nao revelar quais ids existem.
            if (urlObject is null || urlObject.UserId != userId)
            {
                throw new KeyNotFoundException("Url not found.");
            }

            if (urlObject.OriginalUrl != originalUrl)
            {
                urlObject.OriginalUrl = originalUrl;

            }

            if (!string.IsNullOrEmpty(path))
            {
                urlObject.NewUrl = await _urlGeneratorService.GenerateCustomPath(path);
            }

            await _urlRepository.UpdateUrl(urlObject);
        }
        public async Task<Address?> GetUrl(Guid url)
        {
            return  await _urlRepository.GetUrl(url);
        }
        public async Task<Address?> GetUrlRedirect(string shortUrl)
        {
            var url = _urlGeneratorService.FormatUrl(shortUrl);
            return  await _urlRepository.GetUrlRedirect(url);
        }
        public async Task DeleteUrl(Guid id, Guid userId)
        {
            var urlObject = await GetUrl(id);

            // Url de outro usuario responde igual a inexistente, para nao revelar quais ids existem.
            if (urlObject is null || urlObject.UserId != userId)
            {
                throw new KeyNotFoundException("Url not found.");
            }

            await _urlRepository.DeleteUrl(id);
        }
    }
}
