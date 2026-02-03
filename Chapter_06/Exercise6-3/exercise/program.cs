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

        static async Task Main(string[] args)
        {
            AIAgent agent = new AzureOpenAIClient(
                new Uri("<Enter your Azure OpenAI endpoint here>"),
                new AzureCliCredential())
                   .GetChatClient("gpt-4o-mini")
                    .AsIChatClient()
                    .CreateAIAgent(
                        instructions: "You are an expert in explaining technologies in simple English"
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
        }
    }
}
