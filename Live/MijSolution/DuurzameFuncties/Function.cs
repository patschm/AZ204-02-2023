using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DuurzameFuncties
{
    public static class Function
    {
        [FunctionName("Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<Task<string>>();

            // Replace "hello" with the name of your Durable Activity Function.
            var result = context.CallActivityAsync<string>("Activity", "Tokyo");
            outputs.Add(result);
            
            result =  context.CallActivityAsync<string>("Activity", "Seattle");
            outputs.Add(result);

            result =  context.CallActivityAsync<string>("Activity", "London");
            outputs.Add(result);

            await Task.WhenAll(outputs);
            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs.Select(t => t.Result).ToList();
        }

        [FunctionName("Activity")]
        public static async Task<string> SayHelloAsync([ActivityTrigger] string name, ILogger log)
        {
            await Task.Delay(1000);
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("Http_Client")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}