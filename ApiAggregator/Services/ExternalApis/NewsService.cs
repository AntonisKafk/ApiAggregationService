using ApiAggregator.Models;
using ApiAggregator.Services.Interfaces;
using System.Text.Json;

namespace ApiAggregator.Services.ExternalApis
{
    public class NewsService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NewsService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public DataSource ApiSource => DataSource.NewsApi;

        public async Task<IEnumerable<AggregatedItem>> GetDataAsync(string? searchTerm = "null")
        {
            var url = $"https://newsapi.org/v2/everything?q={searchTerm}&sortBy=publishedAt&apiKey={_apiKey}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "ApiAggregatorApp");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RAW JSON from {url}:\n{json}");

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("articles", out JsonElement articles))
            {
                return Enumerable.Empty<AggregatedItem>();
            }

            var result = new List<AggregatedItem>();

            foreach (var article in articles.EnumerateArray())
            {
                var title = article.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? "No Title" : "No Title";
                var link = article.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                var date = article.TryGetProperty("publishedAt", out var dateElement) && dateElement.TryGetDateTime(out var dateValue) ? dateValue : (DateTime?)null;

                result.Add(new AggregatedItem
                {
                    Source = "NewsApi",
                    Title = title,
                    Link = link,
                    Date = date
                });
            }
            return result;

        }
    }
}
