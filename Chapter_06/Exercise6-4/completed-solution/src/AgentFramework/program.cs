using System;
using System.Diagnostics;
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


// Set up the Azure OpenAI client
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ??
    "https://js-aoai-east2.openai.azure.com/";
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();


//Excercise 6.4.1
/******************************************************************************
 * Sequential Execution of Party Planning Agents
 *****************************************************************************/
// 2) Helper method to create party planning agents
//static ChatClientAgent GetPartyPlannerAgent(string targetAgeGroup, IChatClient chatClient) =>
//    new(chatClient,
//        $"You are a party planning specialist who creates event ideas specifically for {targetAgeGroup}. " +
//        $"Always start your response by identifying yourself as '\u001b[36m{GetAgentName(targetAgeGroup)}\u001b[0m' before appending 3 more" +
//        $"creative, age-appropriate party ideas and activities tailored to this demographic.");
//
//static string GetAgentName(string ageGroup) => ageGroup switch
//{
//    "children 5-8 years old" => "Kids Party Planner",
//    "teens" => "Teen Event Coordinator", 
//    "young adults" => "Young Adult Party Expert",
//    "retirees" => "Senior Event Specialist",
//    _ => "Party Planner"
//};

//// Helper method to create summary agent
//static ChatClientAgent GetSummaryAgent(IChatClient chatClient) =>
//    new(chatClient,
//        "You are a summary coordinator who compiles and lists all party activities suggested by other agents. " +
//        "Always start your response by identifying yourself as '\u001b[36mSummary Coordinator\u001b[0m'. " +
//        "Review all the previous suggestions and create a comprehensive list of all activities mentioned, " +
//        "organizing them clearly for easy reference but do not start the count from 1 again.");

//// Create party planning agents for sequential processing
//var partyPlannerAgents = (from ageGroup in (string[])["children 5-8 years old", "teens", "young adults", "retirees"]
//                          select GetPartyPlannerAgent(ageGroup, client));

//// Create summary agent
//var summaryAgent = GetSummaryAgent(client);

//// Combine party planner agents with summary agent
//var allAgents = partyPlannerAgents.Concat([summaryAgent]);

//// 3) Build sequential workflow
//var workflow = AgentWorkflowBuilder.BuildSequential(allAgents);

//// 4) Run the workflow
//var messages = new List<ChatMessage> { new(ChatRole.User, "Plan event in Seattle") };

//// Start timing the sequential execution
//var stopwatch = Stopwatch.StartNew();

//StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
//await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

//List<ChatMessage> result = new();
//await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
//{
//    if (evt is AgentResponseUpdateEvent e)
//    {
//        Console.WriteLine($"{e.ExecutorId}: {e.Data}");
//    }
//    else if (evt is WorkflowOutputEvent outputEvt)
//    {
//        result = (List<ChatMessage>)outputEvt.Data!;
//        stopwatch.Stop();
//        break;
//    }
//}

//// Display final result
//foreach (var message in result)
//{
//    Console.WriteLine($"{message.Role}: {message.Text}");
//}

//// Display runtime
//Console.ForegroundColor = ConsoleColor.Yellow;
//Console.WriteLine($"\n===== Execution Time =====");
//Console.WriteLine($"Sequential agents completed in: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
//Console.ResetColor();


//Exercise 6.4.2
/********************************************************************************
 * Concurrent Execution of Party Planning Agents
 *****************************************************************************/
// 2) Helper method to create party planning agents
static ChatClientAgent GetPartyPlannerAgent(string targetAgeGroup, IChatClient chatClient) =>
    new(chatClient,
        $"You are a party planning specialist who creates event ideas specifically for {targetAgeGroup}. " +
        $"Always start your response by identifying yourself as '\u001b[36m{GetAgentName(targetAgeGroup)}\u001b[0m' before appending 3 more" +
        $"creative, age-appropriate party ideas and activities tailored to this demographic.");

static string GetAgentName(string ageGroup) => ageGroup switch
{
    "children 5-8 years old" => "Kids Party Planner",
    "teens" => "Teen Event Coordinator", 
    "young adults" => "Young Adult Party Expert",
    "women-only" => "Women's Event Specialist",
    "retirees" => "Senior Event Specialist",
    _ => "Party Planner"
};
// Create party planning agents for concurrent processing
var partyPlannerAgents = (from ageGroup in (string[])["children 5-8 years old", "teens", "young adults", "women-only", "retirees"]
                          select GetPartyPlannerAgent(ageGroup, client));

// 3) Build concurrent workflow
var workflow = AgentWorkflowBuilder.BuildConcurrent(partyPlannerAgents);

// 4) Run the workflow
var messages = new List<ChatMessage> { new(ChatRole.User, "Plan event in Seattle") };

// Start timing the concurrent execution
var stopwatch = Stopwatch.StartNew();

StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

List<ChatMessage> result = new();
await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is AgentResponseUpdateEvent e)
    {
        Console.WriteLine($"{e.ExecutorId}: {e.Data}");
    }
    else if (evt is WorkflowOutputEvent outputEvt)
    {
        result = (List<ChatMessage>)outputEvt.Data!;
        stopwatch.Stop();
        break;
    }
}

// Display aggregated results from all agents
Console.WriteLine("===== Final Aggregated Results =====");
foreach (var message in result)
{
    Console.WriteLine($"{message.Role}: {message.Text}");
}


// Display runtime
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"\n===== Execution Time =====");
Console.WriteLine($"Concurrent agents completed in: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
Console.ResetColor();
