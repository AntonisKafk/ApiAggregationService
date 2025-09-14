using ApiAggregator.Models;
using ApiAggregator.Services.Interfaces;
using System.Text.Json;

namespace ApiAggregator.Services.ExternalApis
{
    public class DevToApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;

        public DevToApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public DataSource ApiSource => DataSource.DevToApi;

        public async Task<IEnumerable<AggregatedItem>> GetDataAsync(string? searchTerm = null)
        {
            var url = $"https://dev.to/api/articles?tag={searchTerm}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "ApiAggregatorApp");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"RAW JSON from {url}:\n{json}");

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Enumerable.Empty<AggregatedItem>();
            }

            var result = new List<AggregatedItem>();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var title = item.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? "No Title" : "No Title";
                var link = item.TryGetProperty("url", out var urlElement) ? urlElement.GetString() ?? string.Empty : string.Empty;
                var date = item.TryGetProperty("published_at", out var dateElement) && dateElement.TryGetDateTime(out var dateValue) ? dateValue : (DateTime?)null;

                result.Add(new AggregatedItem
                {
                    Source = "DevToAPI",
                    Title = title,
                    Link = link,
                    Date = date
                });
            }
            return result;

        }
    }
}
