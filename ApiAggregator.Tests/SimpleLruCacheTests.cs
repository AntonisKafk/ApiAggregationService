using ApiAggregator.Models;
using ApiAggregator.Utilities;
namespace ApiAggregator.Tests
{
    public class SimpleLruCacheTests
    {
        [Fact]
        public void AddAndRetrieve_ReturnsStoredValue()
        {
            // Arrange
            var cache = new SimpleLruCache(3);
            var key = "testKey";
            var value = new List<AggregatedItem>
            {
                new AggregatedItem { Source = "TestSource", Title = "TestTitle", Date = DateTime.UtcNow }
            };

            // Act
            cache.AddToCache(key, value);
            var result = cache.GetFromCache(key);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Should contain exactly one item
            Assert.Equal("TestTitle", result.First().Title);
            Assert.Equal("TestSource", result.First().Source);
        }

        [Fact]
        public void UpdateExistingKey()
        {
            // Arrange
            var cache = new SimpleLruCache(2);
            var key = "testKey";
            var value = new List<AggregatedItem>
            {
                new AggregatedItem { Source = "TestSource", Title = "TestTitle", Date = DateTime.UtcNow }
            };

            // Act
            cache.AddToCache(key, value);
            var updatedValue = new List<AggregatedItem>
            {
                new AggregatedItem { Source = "TestSource", Title = "UpdatedTitle", Date = DateTime.UtcNow }
            };
            cache.AddToCache(key, updatedValue);
            var result = cache.GetFromCache(key);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Should contain exactly one item    
            Assert.Equal("UpdatedTitle", result.First().Title); // Should reflect updated value
        }

        [Fact]
        public void ExceedsCapacity_EvictsLeastRecentlyUsed()
        {
            // Arrange
            var cache = new SimpleLruCache(2);
            var key1 = "key1";
            var key2 = "key2";
            var key3 = "key3";
            var value = new List<AggregatedItem>
            {
                new AggregatedItem { Source = "TestSource", Title = "TestTitle", Date = DateTime.UtcNow }
            };

            // Act
            cache.AddToCache(key1, value);
            cache.AddToCache(key2, value);
            cache.GetFromCache(key1); // Access key1 to make it recently used
            cache.AddToCache(key3, value); // This should evict key2
            var result1 = cache.GetFromCache(key1);
            var result2 = cache.GetFromCache(key2);
            var result3 = cache.GetFromCache(key3);

            // Assert
            Assert.NotNull(result1); // key1 should still be present
            Assert.Null(result2);    // key2 should have been evicted
            Assert.NotNull(result3); // key3 should be present
        }

        [Fact]
        public void RetrieveNonExistentKey_ReturnsNull()
        {
            // Arrange
            var cache = new SimpleLruCache(2);
            var key = "nonExistentKey";
            // Act
            var result = cache.GetFromCache(key);
            // Assert
            Assert.Null(result); // Should return null for non-existent key
        }

        [Fact]
        public void CapacityZero_ReturnsArgumentOutOfRangeException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new SimpleLruCache(0));
        }

    }
}
