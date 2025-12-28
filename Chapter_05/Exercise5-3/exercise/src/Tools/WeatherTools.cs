using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace McpServer.Tools;

[McpServerToolType]
public class WeatherTools
{
    private const string NWS_API_BASE = "https://api.weather.gov";
    private static readonly HttpClient _httpClient = new HttpClient()
    {
        BaseAddress = new Uri(NWS_API_BASE)
    };

    static WeatherTools()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "McpServer-Weather/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/geo+json");
    }

    [McpServerTool, Description("Get weather alerts for a US state.")]
    public static async Task<string> GetAlerts(
        [Description("The US state to get alerts for.")] string state)
    {
        try
        {
            var jsonElement = await _httpClient.GetFromJsonAsync<JsonElement>($"/alerts/active/area/{state}");
            
            if (!jsonElement.TryGetProperty("features", out var featuresElement))
            {
                return "Unable to fetch alerts or no alerts found.";
            }

            var alerts = featuresElement.EnumerateArray();

            if (!alerts.Any())
            {
                return "No active alerts for this state.";
            }

            return string.Join("\n--\n", alerts.Select(alert =>
            {
                JsonElement properties = alert.GetProperty("properties");
                return $"""
                        Event: {properties.GetProperty("event").GetString()}
                        Area: {properties.GetProperty("areaDesc").GetString()}
                        Severity: {properties.GetProperty("severity").GetString()}
                        Description: {properties.GetProperty("description").GetString()}
                        Instruction: {TryGetString(properties, "instruction")}
                        """;
            }));
        }
        catch (Exception ex)
        {
            return $"Error fetching weather alerts: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get weather forecast for a location.")]
    public static async Task<string> GetForecast(
        [Description("Latitude of the location.")] double latitude,
        [Description("Longitude of the location.")] double longitude)
    {
        try
        {
            // First get the forecast grid endpoint
            var pointsData = await _httpClient.GetFromJsonAsync<JsonElement>($"/points/{latitude},{longitude}");
            
            if (!pointsData.TryGetProperty("properties", out var properties))
            {
                return "Unable to fetch forecast data for this location.";
            }

            // Get the forecast URL from the points response
            string forecastUrl = properties.GetProperty("forecast").GetString()!;
            
            // Make a request to the forecast URL
            var forecastData = await _httpClient.GetFromJsonAsync<JsonElement>(forecastUrl);
            
            if (!forecastData.TryGetProperty("properties", out var forecastProps) || 
                !forecastProps.TryGetProperty("periods", out var periodsElement))
            {
                return "Unable to fetch detailed forecast.";
            }

            var periods = periodsElement.EnumerateArray();

            // Format the periods into a readable forecast (limit to first 5 periods)
            return string.Join("\n---\n", periods.Take(5).Select(period => $"""
                    {period.GetProperty("name").GetString()}
                    Temperature: {period.GetProperty("temperature").GetInt32()}°{period.GetProperty("temperatureUnit").GetString()}
                    Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
                    Forecast: {period.GetProperty("detailedForecast").GetString()}
                    """));
        }
        catch (Exception ex)
        {
            return $"Error fetching weather forecast: {ex.Message}";
        }
    }

    [McpServerTool, Description("Get a list of available radar servers/stations from the National Weather Service.")]
    public static async Task<string> GetRadarStations()
    {
        try
        {
            var jsonElement = await _httpClient.GetFromJsonAsync<JsonElement>("/radar/stations");
            
            if (!jsonElement.TryGetProperty("features", out var featuresElement))
            {
                return "Unable to fetch radar station data.";
            }

            var stations = featuresElement.EnumerateArray();

            if (!stations.Any())
            {
                return "No radar stations found.";
            }

            return string.Join("\n", stations.Select(station =>
            {
                JsonElement properties = station.GetProperty("properties");
                JsonElement geometry = station.GetProperty("geometry");
                JsonElement coordinates = geometry.GetProperty("coordinates");
                
                // More robust coordinate parsing
                double lat = coordinates.GetArrayLength() > 1 ? coordinates[1].GetDouble() : 0.0;
                double lon = coordinates.GetArrayLength() > 0 ? coordinates[0].GetDouble() : 0.0;
                
                // More robust elevation parsing
                string elevation = TryGetProperty(properties, "elevation");
                if (string.IsNullOrEmpty(elevation) && properties.TryGetProperty("elevation", out var elevProp))
                {
                    elevation = elevProp.ValueKind == JsonValueKind.Number ? 
                        elevProp.GetDouble().ToString("F0") : 
                        elevProp.ToString();
                }
                
                return $"""
                        Station: {TryGetString(properties, "stationIdentifier")} - {TryGetString(properties, "name")}
                        Location: {TryGetString(properties, "city")}, {TryGetString(properties, "state")}
                        Coordinates: {lat:F4}, {lon:F4} (Lat, Lon)
                        Elevation: {elevation} ft
                        Type: {TryGetString(properties, "radarType")}
                        Status: {TryGetString(properties, "operationalStatus")}
                        """;
            }));
        }
        catch (Exception ex)
        {
            return $"Error fetching radar stations: {ex.Message}";
        }
    }

    // Helper method to safely get string values from JsonElement
    private static string TryGetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && 
            property.ValueKind != JsonValueKind.Null)
        {
            return property.GetString() ?? string.Empty;
        }
        return string.Empty;
    }
    
    // Helper method to safely get property values as strings with better handling
    private static string TryGetProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && 
            property.ValueKind != JsonValueKind.Null)
        {
            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString() ?? string.Empty,
                JsonValueKind.Number => property.GetDouble().ToString("F0"),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => property.ToString()
            };
        }
        return string.Empty;
    }
}
