namespace ApiAggregator.Services.Interfaces
{
    using ApiAggregator.Models;
    public interface IExternalApiService
    {
        DataSource ApiSource { get; }
        Task<IEnumerable<AggregatedItem>> GetDataAsync(string? searchTerm = null);
    }
}
