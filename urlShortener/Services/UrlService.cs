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
        public async Task UpdateUrl(Guid id, string originalUrl, string path)
        {

            if (String.IsNullOrEmpty(originalUrl))
            {
                throw new InvalidOperationException("Url cannot de null.");
            }
            try
            {
                var urlObject = GetUrl(id);

                if (urlObject.Result.OriginalUrl != originalUrl)
                {
                    urlObject.Result.OriginalUrl = originalUrl;

                }

                if (path != null)
                {
                    urlObject.Result.NewUrl = await _urlGeneratorService.GenerateCustomPath(path);
                }

                await _urlRepository.UpdateUrl(urlObject.Result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public async Task<Address> GetUrl(Guid url)
        {
            return  await _urlRepository.GetUrl(url);
        } 
        public async Task<Address> GetUrlRedirect(string shortUrl)
        {
            var url = _urlGeneratorService.FormatUrl(shortUrl);
            return  await _urlRepository.GetUrlRedirect(url);
        }
        public async Task DeleteUrl(Guid id)
        {
            try
            {
                await _urlRepository.DeleteUrl(id);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Url not found.");
            }
        }
    }
}
