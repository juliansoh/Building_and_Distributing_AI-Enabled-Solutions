using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading.Tasks;
using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace WeatherApp
{
    class Program
    {
        [Description("Get the weather for a given location.")]
        static async Task<string> GetWeather([Description("Latitude of the location")] double latitude, [Description("Longitude of the location")] double longitude)
        {
            var service = new WeatherService();
            var weather = await service.GetWeatherAsync(latitude, longitude);
            return $"Weather for {weather.Name}: {weather.Temperature}°{weather.TemperatureUnit}, {weather.DetailedForecast}";
        }

        static async Task Main(string[] args)
        {
            AIAgent agent = new AzureOpenAIClient(
                new Uri("<Enter your Azure OpenAI endpoint here>"),
                new AzureCliCredential())
                   .GetChatClient("gpt-4o-mini")
                    .AsIChatClient()
                    .CreateAIAgent(
                        instructions: "You are an expert in explaining technologies in simple English", tools: [AIFunctionFactory.Create(GetWeather)]
                    );

            //Create a conversation thread
            AgentThread thread = agent.GetNewThread();

            Console.WriteLine("Starting conversation. Type 'end' to exit.");
            Console.WriteLine();

            //Conversation loop
            while (true)
            {
                Console.Write("You: ");
                string? userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.Trim().Equals("end", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Conversation ended.");
                    break;
                }

                // Send user input to the agent within the same thread
                string response = (await agent.RunAsync(userInput, thread)).ToString();

                Console.WriteLine();
                Console.WriteLine("Agent:");
                Console.WriteLine(response);
                Console.WriteLine();
            }
            /*
            Console.Write("Enter latitude: ");
            double latitude = double.Parse(Console.ReadLine());

            Console.Write("Enter longitude: ");
            double longitude = double.Parse(Console.ReadLine());

            var service = new WeatherService();
            var weather = await service.GetWeatherAsync(latitude, longitude);

            Console.WriteLine("\n--- Weather Report ---");
            Console.WriteLine($"Location: {weather.Name}");
            Console.WriteLine($"Temperature: {weather.Temperature} {weather.TemperatureUnit}");
            Console.WriteLine($"Wind: {weather.WindSpeed} {weather.WindDirection}");
            Console.WriteLine($"Forecast: {weather.DetailedForecast}");
            */
        }
    }
}
