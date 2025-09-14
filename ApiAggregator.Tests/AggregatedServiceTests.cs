using ApiAggregator.Models;
using ApiAggregator.Services;
using ApiAggregator.Services.Interfaces;
using Moq;

namespace ApiAggregator.Tests
{
    public class AggregatedServiceTests
    {
        private readonly AggregatedService _aggregatedService;
        private readonly Mock<IExternalApiService> _mockApi1;
        private readonly Mock<IExternalApiService> _mockApi2;

        public AggregatedServiceTests()
        {
            // Mock Api 1
            _mockApi1 = new Mock<IExternalApiService>();
            _mockApi1.Setup(x => x.ApiSource).Returns(DataSource.NewsApi);
            _mockApi1.Setup(x => x.GetDataAsync(It.IsAny<string>()))
                   .ReturnsAsync(new List<AggregatedItem>
                   {
                         new AggregatedItem { Source = "NewsApi", Title = "Article 1", Date = DateTime.UtcNow.AddHours(-2) },
                         new AggregatedItem { Source = "NewsApi", Title = "Article 2", Date = DateTime.UtcNow }
                   });

            // Mock Api 2
            _mockApi2 = new Mock<IExternalApiService>();
            _mockApi2.Setup(x => x.ApiSource).Returns(DataSource.DevToApi);
            _mockApi2.Setup(x => x.GetDataAsync(It.IsAny<string>()))
                     .ReturnsAsync(new List<AggregatedItem>
                     {
                         new AggregatedItem { Source = "DevToAPI", Title = "Post 1", Date = DateTime.UtcNow.AddHours(-1) }
                     });

            // Aggregated service with mocked APIs
            _aggregatedService = new AggregatedService(new List<IExternalApiService> { _mockApi1.Object, _mockApi2.Object });
        }

        [Fact]
        public async Task GetAggregatedDataAsync_ReturnsAllItemsFromMultipleApis()
        {
            var results = await _aggregatedService.GetAggregatedDataAsync();

            Assert.Equal(3, results.Count()); // 2 from API1 + 1 from API2
            Assert.Contains(results, x => x.Source == "NewsApi" && x.Title == "Article 1");
            Assert.Contains(results, x => x.Source == "DevToAPI" && x.Title == "Post 1");
        }

        [Fact]
        public async Task GetAggregatedDataAsync_SortsByDateAscending()
        {
            var results = await _aggregatedService.GetAggregatedDataAsync(dateOrder: DateSortOrder.Ascending);

            var ordered = results.OrderBy(x => x.Date).ToList();
            Assert.Equal(ordered, results.ToList()); // Should match ascending order
        }

        [Fact]
        public async Task GetAggregatedDataAsync_SortsByDateDescending()
        {
            var results = await _aggregatedService.GetAggregatedDataAsync(dateOrder: DateSortOrder.Descending);

            var ordered = results.OrderByDescending(x => x.Date).ToList();
            Assert.Equal(ordered, results.ToList()); // Should match descending order
        }

        [Fact]
        public async Task GetAggregatedDataAsync_FiltersByDataSource()
        {
            var results = await _aggregatedService.GetAggregatedDataAsync(dataSources: new List<DataSource> { DataSource.NewsApi });

            Assert.All(results, x => Assert.Equal("NewsApi", x.Source));
        }

        [Fact]
        public async Task GetAggregatedDataAsync_ReturnsFallbackItem_WhenApiThrowsException()
        {
            // Arrange
            _mockApi1.Setup(x => x.GetDataAsync(It.IsAny<string>()))
                     .ThrowsAsync(new Exception("API failure"));

            // Act
            var results = await _aggregatedService.GetAggregatedDataAsync();

            // Assert
            Assert.Contains(results, x => x.Title.Contains("unavailable - fallback response"));
            Assert.Contains(results, x => x.Source == DataSource.NewsApi.ToString());
        }

        [Fact]
        public async Task GetAggregatedDataAsync_UsesCache_OnSubsequentCalls()
        {
            var searchTerm = "dotnet";
            // First call to populate cache
            var firstCallResults = await _aggregatedService.GetAggregatedDataAsync();
            // Second call should hit the cache
            var secondCallResults = await _aggregatedService.GetAggregatedDataAsync();

            //Verify consintency in cache
            Assert.Equal(firstCallResults.Count(), secondCallResults.Count());
            Assert.True(firstCallResults.All(item => secondCallResults.Any(i => i.Title == item.Title && i.Source == item.Source)));


            // Verify that each API's GetDataAsync was called only once
            _mockApi1.Verify(x => x.GetDataAsync(It.IsAny<string>()), Times.Once); // Still once
            _mockApi2.Verify(x => x.GetDataAsync(It.IsAny<string>()), Times.Once); // Still once
        }

        [Fact]
        public async Task GetAggregatedDataAsync_ReturnsEmptyList_WhenApiReturnsNoData()
        {
            _mockApi1.Setup(x => x.GetDataAsync(It.IsAny<string>()))
                     .ReturnsAsync(new List<AggregatedItem>());

            var results = await _aggregatedService.GetAggregatedDataAsync(dataSources: new List<DataSource> { DataSource.NewsApi});

            Assert.Empty(results);
        }

        [Fact]
        public async Task GetAggregatedDataAsync_NoApisConfigured()
        {
            var service = new AggregatedService(new List<IExternalApiService>());
            var results = await service.GetAggregatedDataAsync();
            Assert.Empty(results);
        }

    }
}
