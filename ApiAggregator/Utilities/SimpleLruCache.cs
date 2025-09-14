using ApiAggregator.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace ApiAggregator.Utilities
{
    public class SimpleLruCache
    {
        private readonly int _capacity;
        private readonly Dictionary<string, IEnumerable<AggregatedItem>> _cache = new();
        private readonly LinkedList<string> _usageOrder = new();
        public SimpleLruCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than zero.");
            }
            _capacity = capacity;
        }
        
        public IEnumerable<AggregatedItem>? GetFromCache(string key)
        {
            if(_cache.TryGetValue(key, out var value))
            {
                // Move the accessed key to the front (most recently used)
                _usageOrder.Remove(key);
                _usageOrder.AddFirst(key);
                return value;
            }
            return null;
        }

        public void AddToCache(string key, IEnumerable<AggregatedItem> value) 
        {
            if(_cache.ContainsKey(key))
            {
                // Update existing key and move to front
                _cache[key] = value;
                _usageOrder.Remove(key);
                _usageOrder.AddFirst(key);
            }
            else
            {
                if(_cache.Count >= _capacity)
                {
                    var lruKey = _usageOrder.Last!.Value;
                    _usageOrder.RemoveLast();
                    _cache.Remove(lruKey);
                }
                // Add new key-value pair
                _cache[key] = value;
                _usageOrder.AddFirst(key);
            }
        }
    }
}
