using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace WeatherApp
{
    public class WeatherService
    {
        private readonly HttpClient _http;

        public WeatherService()
        {
            // Configure HttpClientHandler to bypass SSL certificate validation (development only!)
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            
            _http = new HttpClient(handler);
            _http.DefaultRequestHeaders.Add("User-Agent", "WeatherAppSample (your-email@example.com)");
        }

        public async Task<WeatherResult> GetWeatherAsync(double lat, double lon)
        {
            // Step 1: Get the forecast URL
            string pointUrl = $"https://api.weather.gov/points/{lat},{lon}";
            var pointJson = await _http.GetFromJsonAsync<JsonElement>(pointUrl);

            string forecastUrl = pointJson
                .GetProperty("properties")
                .GetProperty("forecast")
                .GetString();

            // Step 2: Get the forecast data
            var forecastJson = await _http.GetFromJsonAsync<JsonElement>(forecastUrl);

            var current = forecastJson
                .GetProperty("properties")
                .GetProperty("periods")[0];

            return new WeatherResult
            {
                Name = current.GetProperty("name").GetString(),
                Temperature = current.GetProperty("temperature").GetInt32(),
                TemperatureUnit = current.GetProperty("temperatureUnit").GetString(),
                WindSpeed = current.GetProperty("windSpeed").GetString(),
                WindDirection = current.GetProperty("windDirection").GetString(),
                DetailedForecast = current.GetProperty("detailedForecast").GetString()
            };
        }
    }

    public class WeatherResult
    {
        public string Name { get; set; }
        public int Temperature { get; set; }
        public string TemperatureUnit { get; set; }
        public string WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public string DetailedForecast { get; set; }
    }
}
