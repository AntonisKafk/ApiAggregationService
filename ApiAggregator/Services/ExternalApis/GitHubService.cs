using ApiAggregator.Models;
using ApiAggregator.Services.Interfaces;
using System.Text.Json;

namespace ApiAggregator.Services.ExternalApis
{
    public class GitHubService : IExternalApiService
    {
        private readonly HttpClient _httpClient;

        public GitHubService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public DataSource ApiSource => DataSource.GitHub;

        public async Task<IEnumerable<AggregatedItem>> GetDataAsync(string? searchTerm = "null")
        {
            var url = $"https://api.github.com/search/repositories?q={searchTerm}&sort=stars&order=desc";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "ApiAggregatorApp");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RAW JSON from {url}:\n{json}");

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out JsonElement items))
            {
                return Enumerable.Empty<AggregatedItem>();
            }

            var result = new List<AggregatedItem>();

            foreach (var item in items.EnumerateArray())
            {
                var title = item.TryGetProperty("full_name", out var titleElement) ? titleElement.GetString() ?? "No Title" : "No Title";
                var link = item.TryGetProperty("html_url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                var date = item.TryGetProperty("created_at", out var dateElement) && dateElement.TryGetDateTime(out var dateValue) ? dateValue : (DateTime?)null;

                result.Add(new AggregatedItem
                {
                    Source = "GitHub",
                    Title = title,
                    Link = link,
                    Date = date
                });
            }
            return result;

        }
    }
}
