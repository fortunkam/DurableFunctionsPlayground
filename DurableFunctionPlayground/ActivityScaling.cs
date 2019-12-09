using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionPlayground
{
    public static class ActivityScaling
    {
        [FunctionName("ActivityScaling")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var tasks = new List<Task<string>>();
            for (int i = 0; i < 10000; i++)
            {
                // Replace "hello" with the name of your Durable Activity Function.
                tasks.Add(context.CallActivityAsync<string>("ActivityScaling_Hello", ""));
            }

            await Task.WhenAll(tasks);
            
            return outputs;
        }

        [FunctionName("ActivityScaling_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            Thread.Sleep(30000);
            return $"Hello {name}!";
        }

        [FunctionName("ActivityScaling_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("ActivityScaling", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}