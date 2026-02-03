using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.OpenAI;


            
namespace WeatherApp
{

    class Program
    {

       //Define the GetWeather function with appropriate descriptions
        [Description("Get the weather for a given location.")]
        static async Task<string> GetWeather([Description("Latitude of the location")] double latitude, [Description("Longitude of the location")] double longitude)
        {
            var service = new WeatherService();
            var weather = await service.GetWeatherAsync(latitude, longitude);
            return $"Weather for {weather.Name}: {weather.Temperature}°{weather.TemperatureUnit}, {weather.DetailedForecast}";
        }

        static async Task ProcessWorkflowAsync(Microsoft.Agents.AI.Workflows.Workflow workflow, List<ChatMessage> messages)
        {
            StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is Microsoft.Agents.AI.Workflows.AgentResponseUpdateEvent update)
                {
                    // Process streaming agent responses
                    AgentResponse response = update.AsResponse();
                    foreach (ChatMessage message in response.Messages)
                    {
                        Console.WriteLine($"[{update.ExecutorId}]: {message.Text}");
                    }
                }
                else if (evt is WorkflowOutputEvent output)
                {
                    // Workflow completed
                    var conversationHistory = output.As<List<ChatMessage>>();
                    Console.WriteLine("\n=== Final Conversation ===");
                    foreach (var message in conversationHistory)
                    {
                        Console.WriteLine($"[{message.AuthorName}]: {message.Text}");
                    }
                    break;
                }
            }
        }

        static async Task Main(string[] args)
        {
            // Set up the Azure OpenAI client
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ??
                "https://js-aoai-east2.openai.azure.com/";
            var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
                .GetChatClient(deploymentName)
                .AsIChatClient();
            
            //Initialize WeatherAgent
            ChatClientAgent WeatherAgent = new(client,
                "You use tools to get the current weather and provide feedback on weather appropriate activities.",
                "WeatherAgent",
                "A weather information agent",
                tools: [AIFunctionFactory.Create(GetWeather)]);

            //Define additional agents
            // Create an Event Planner Agent
            ChatClientAgent EventPlannerAgent = new(client,
                "You are an event planner for activities in a US city. Generate interesting activities available in a city.",
                "EventPlanner",
                "An event planning agent");

            // Create a reviewer agent
            ChatClientAgent ReviewerAgent = new(client,
                "You are a reviewer of *WEATHER APPROPRIATE* planned events. Evaluate the feasibility by *ASKING WeatherAgent* what the weather is, appeal, and logistics of the proposed activities. " +
                "Provide constructive feedback or approval.",
                "Reviewer",
                "An event review agent");

            // Build group chat with round-robin speaker selection
            // The manager factory receives the list of agents and returns a configured manager
            var workflow = AgentWorkflowBuilder
                .CreateGroupChatBuilderWith(agents =>
                    new RoundRobinGroupChatManager(agents)
                    {
                        MaximumIterationCount = 5  // Maximum number of turns
                    })
                .AddParticipants(EventPlannerAgent, ReviewerAgent, WeatherAgent)
                .Build();                

            // Start conversation loop
            Console.WriteLine("Starting conversation. Type 'end' to exit.");
            Console.WriteLine();

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

                var messages = new List<ChatMessage> {
                    new(ChatRole.User, userInput)
                };

                await ProcessWorkflowAsync(workflow, messages);
                Console.WriteLine();
            }
        }
    }
}
