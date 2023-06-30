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

            try
            {
                string repo = req.Query["repo"];
                string owner = req.Query["owner"];

                if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    repo = req.Form["repository"];
                    owner = req.Form["owner"];
                }

                string accessToken = Environment.GetEnvironmentVariable("GithubAccessToken");

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await httpClient.GetAsync($"https://api.github.com/repos/{owner}/{repo}/commits");

                var commits = await response.Content.ReadAsStringAsync();

                var latestCommit = JsonConvert.DeserializeObject<CommitData[]>(commits)[0];

                var message = $"Latest commit: {latestCommit.Commit.Message}" + Environment.NewLine;
                message += $"Author: {latestCommit.Commit.Author.Name}" + Environment.NewLine;
                message += $"Commit URL: {latestCommit.HtmlUrl}";

                var teamsWebhookUrl = Environment.GetEnvironmentVariable("TeamsWebhookUrl");
                var payload = new { text = commits };
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(jsonPayload);
                await httpClient.PostAsync(teamsWebhookUrl, httpContent);

                return new OkObjectResult(commits);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "error");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private class CommitData
        {
            public CommitDetails Commit { get; set; }
            public string HtmlUrl { get; set; }
        }

        private class CommitDetails
        {
            public string Message { get; set; }
            public AuthorDetails Author { get; set; }
        }

        private class AuthorDetails
        {
            public string Name { get; set; }
        }
    }
}

