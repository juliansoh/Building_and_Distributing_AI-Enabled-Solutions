using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

AIAgent agent = new AzureOpenAIClient(
    new Uri("<Enter your Azure OpenAI endpoint here>"),
    new AzureCliCredential())
        .GetChatClient("gpt-4o-mini")
        .AsIChatClient()
        .CreateAIAgent(instructions: "You are an expert in explaining technologies in simple English");

//Exercise 6.2.1 - Conversational Agent

// //Create a conversation thread
//AgentThread thread = agent.GetNewThread();

//Console.WriteLine("Starting conversation. Type 'end' to exit.");
//Console.WriteLine();

// //Conversation loop
//while (true)
//{
//    Console.Write("You: ");
//    string? userInput = Console.ReadLine();

//    if (string.IsNullOrWhiteSpace(userInput))
//        continue;

//    if (userInput.Trim().Equals("end", StringComparison.OrdinalIgnoreCase))
//    {
//        Console.WriteLine("Conversation ended.");
//        break;
//    }

//    // Send user input to the agent within the same thread
//    string response = (await agent.RunAsync(userInput, thread)).ToString();
//    Console.WriteLine();
//    Console.WriteLine("Agent:");
//    Console.WriteLine(response);
//    Console.WriteLine();
//}

// Exercise 6.2.2 - Multi-threaded Conversations

//AgentThread thread1 = agent.GetNewThread();
//AgentThread thread2 = agent.GetNewThread();
//Console.WriteLine("----------------- Conversation 1 -----------------");
//Console.WriteLine(await agent.RunAsync("Tell me about Model Context Protocol.", thread1));
//Console.WriteLine();
//Console.WriteLine("----------------- Conversation 2 -----------------");
//Console.WriteLine(await agent.RunAsync("Create a one paragraph plot of a sci-fi movie.", thread2));
//Console.WriteLine();
//Console.WriteLine("----------------- Conversation 1 -----------------");
//Console.WriteLine(await agent.RunAsync("How are tools defined?", thread1));
//Console.WriteLine();
//Console.WriteLine("----------------- Conversation 2 -----------------");
//Console.WriteLine(await agent.RunAsync("Mix the sci-fi plot with some fantasy, like in Piers Anthony's Split Infinity trilogy.", thread2));
//Console.WriteLine();
//Console.WriteLine("----------------- Conversation 1 -----------------");
//Console.WriteLine(await agent.RunAsync("What have been talking about so far? Start your response with Thread 1:", thread1));
//Console.WriteLine();
//Console.WriteLine("----------------- Conversation 2 -----------------");
//Console.WriteLine(await agent.RunAsync("What have been talking about so far? Start your response with Thread 2:", thread2));
//Console.WriteLine();
