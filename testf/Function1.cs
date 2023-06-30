using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace testf
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string repository = req.Query["repository"];

            if (string.IsNullOrEmpty(repository))
            {
                repository = req.Form["repository"];
            }

            string accessToken = Environment.GetEnvironmentVariable("GithubAccessToken");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = httpClient.GetAsync($"https://api.github.com/repos/{repository}/commits").Result;

            var commits = response.Content.ReadAsStringAsync().Result;


            return new OkObjectResult("Get the commit and send the message successfully!");
        }
    }
}
