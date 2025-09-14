using ApiAggregator.Models;
using ApiAggregator.Services.Interfaces;
using ApiAggregator.Utilities;

namespace ApiAggregator.Services
{
    public class AggregatedService
    {
        private readonly IEnumerable<IExternalApiService> _externalServices;
        private readonly Dictionary<DataSource, SimpleLruCache> _apiCaches;

        public AggregatedService(IEnumerable<IExternalApiService> externalServices)
        {
            _externalServices = externalServices;
            _apiCaches = _externalServices.ToDictionary(
                service => service.ApiSource,
                service => new SimpleLruCache(10) // Cache size of 10 items per API
            );
        }

        public async Task<IEnumerable<Models.AggregatedItem>> GetAggregatedDataAsync(string? searchTerm = null, DateSortOrder dateOrder = DateSortOrder.Descending, List<DataSource>? dataSources = null)
        {
            var servicesToBeCalled = _externalServices;
            if(dataSources != null && dataSources.Count > 0)
            {
                servicesToBeCalled = _externalServices.Where(x => dataSources.Contains(x.ApiSource));
            }
            var tasks = servicesToBeCalled.Select(async service => 
            {
                var cacheKey = searchTerm ?? "default";
                var cachedData = _apiCaches[service.ApiSource].GetFromCache(cacheKey);
                if (cachedData != null)
                {
                    return cachedData;
                }

                try
                {
                    var data = await service.GetDataAsync(searchTerm);
                    if(data != null)
                    {
                        _apiCaches[service.ApiSource].AddToCache(cacheKey.Trim().ToLower(), data);
                    }
                    return data;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Service {service.ApiSource} failed with error: {ex.Message}");
                    return new List<AggregatedItem>
                    {
                        new AggregatedItem
                        {
                            Source = service.ApiSource.ToString(),
                            Title = $"{service.ApiSource} unavailable - fallback response",
                            Link = string.Empty,
                            Date = DateTime.UtcNow
                        }
                    };
                }
            });

            var results = await Task.WhenAll(tasks);
            var aggregatedList = results.SelectMany(r => r);

            if (dateOrder == DateSortOrder.Ascending)
            {
                return aggregatedList.OrderBy(item => item.Date).ToList();
            }
            else
            {
                return aggregatedList.OrderByDescending(item => item.Date).ToList();
            }
        }
    }
}
