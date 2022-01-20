using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Options;
using Sorigin.Settings;
using System.Net;

namespace Sorigin.Services;

public interface ILocationService
{
    /// <summary>
    /// Returns the ISO code of the location of an IP, or null if it could not be found.
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    Task<string?> GetLocationAsync(string ipAddress);
}

public class LocationService : ILocationService, IDisposable
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IAdminSettings _adminSettings;
    private readonly WebServiceClient _geoIP2Provider;
    private readonly IMaxMindSettings _maxMindSettings;

    public LocationService(ILogger logger, HttpClient httpClient, IAdminSettings adminSettings, IMaxMindSettings maxMindSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _adminSettings = adminSettings;
        _maxMindSettings = maxMindSettings;
        _geoIP2Provider = new WebServiceClient(_httpClient, Options.Create(new WebServiceClientOptions
        {
            AccountId = _maxMindSettings.ID,
            LicenseKey = _maxMindSettings.Key,
            Host = "geolite.info",
            Timeout = 30000
        }));
    }

    public async Task<string?> GetLocationAsync(string ipAddress)
    {
        // We do not store IP information, so we don't include it in the log.
        // Storing IPs would be a bit weird ngl.
        _logger.LogInformation("Performing lookup for an IP.");

        try
        {
            // If it's a local IP performing the request, we override it with our proxy IP.
            if (ipAddress == "127.0.0.1" || ipAddress == "::1" || string.IsNullOrWhiteSpace(ipAddress))
            {
                _logger.LogInformation("Updating location request with proxy IP.");
                ipAddress = _adminSettings.ProxyIP;
            }

            CountryResponse? countryResponse = await _geoIP2Provider.CountryAsync(ipAddress);
            if (countryResponse is null)
            {
                _logger.LogWarning("Unable to locate IP.");
                return null;
            }

            return countryResponse.Country.IsoCode;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not get location data.");
            return null;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _geoIP2Provider.Dispose();
    }
}
