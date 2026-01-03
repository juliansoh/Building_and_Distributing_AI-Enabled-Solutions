using System;
using System.ComponentModel.DataAnnotations;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

AIAgent agent = new AzureOpenAIClient(
  new Uri("https://js-aoai-east2.openai.azure.com/"),
  new AzureCliCredential())
    .GetChatClient("gpt-4o-mini")
    .AsIChatClient()
    .CreateAIAgent(instructions: "You are an expert in explaining technologies in simple English");

//Exercise 6.1.1 - Simple Agent
//Console.WriteLine(await agent.RunAsync("Tell me about Model Context Protocol."));

//Exercise 6.1.2 - Simple Agent with Streaming
//int count = 0;
//await foreach (var update in agent.RunStreamingAsync("Tell me about Model Context Protocol."))
//{
//    string strUpdate = update.Text;
//    if (count < 100)
//    {
//        Console.Write(update);
//        //increment count based on number of words printed
//        count += strUpdate.Split(' ').Length;
//    }
//    else
//    {
//        Console.WriteLine(update);
//        count = 0;
//    }
//}

//Exercise 6.1.3 - Passing multiple objects (including an image) to the Agent
ChatMessage systemMessage = new(
    ChatRole.System,
    """
    If asked to respond in any language other than English, 
    then provide the response in the requested language
    and translate the response to English and append as part of the entire response.
    """);

ChatMessage userMessage1 = new(ChatRole.User, [
    new TextContent("Describe this image in detail in Korean:"),
    new UriContent("https://upload.wikimedia.org/wikipedia/commons/1/11/Joseph_Grimaldi.jpg", "image/jpeg")
]);

Console.WriteLine("--------- UserMessage1 Response ---------");
Console.WriteLine(await agent.RunAsync([systemMessage, userMessage1]));
Console.WriteLine("");

ChatMessage userMessage2 = new(ChatRole.User, [
    new TextContent("Describe this image in detail in Simplified Chinese:"),
    new UriContent("https://upload.wikimedia.org/wikipedia/commons/1/11/Joseph_Grimaldi.jpg", "image/jpeg")
]);

Console.WriteLine("--------- UserMessage2 Response ---------");

int count = 0;
await foreach (var update in agent.RunStreamingAsync([systemMessage, userMessage2]))
{
    string strUpdate = update.Text;
    if (count < 100)
    {
        Console.Write(update);
        //increment count based on number of words printed
        count += strUpdate.Split(' ').Length;
    }
    else
    {
        Console.WriteLine(update);
        count = 0;
    }
}

//Console.WriteLine(await agent.RunAsync([systemMessage, userMessage2]));
Console.WriteLine("");
