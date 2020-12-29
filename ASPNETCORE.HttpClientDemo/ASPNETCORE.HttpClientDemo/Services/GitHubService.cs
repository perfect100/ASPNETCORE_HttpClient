using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASPNETCORE.HttpClientDemo.Services
{
    public class GitHubService
    {
        private readonly HttpClient client;


        public GitHubService(HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            this.client = client;
        }

        public HttpClient Client => client;
        
        public async Task<IEnumerable<GitHubIssue>> GetAspNetDocsIssues()
        {
            var response = await Client.GetAsync(
                "/repos/aspnet/AspNetCore.Docs/issues?state=open&sort=created&direction=desc");

            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync
                <IEnumerable<GitHubIssue>>(responseStream);
        }
    }

    public class GitHubIssue
    {
    }
}