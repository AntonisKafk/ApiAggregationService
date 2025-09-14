# API Aggregator

## Overview
This project is a .NET 8 ASP.NET Core API Aggregation service that consolidates data from multiple external APIs and provides a unified endpoint to access the aggregated information. The service is designed for easy integration of new APIs, with caching, error handling, and fallback mechanisms built-in.

---

## Features
- Aggregates data from multiple external APIs simultaneously.
- Filter results by search term and source.
- Sort aggregated results by date ascending or descending.
- Fallback responses when any external API is unavailable.
- Caching using a simple LRU cache to reduce redundant API calls.
- Fully unit-tested service and cache functionality.
- Easily extensible architecture for adding new APIs.

---

## Architecture
```
ApiAggregatorSolution/
│
├─ ApiAggregator/ # Main API project
│ ├─ Controllers/ # API controllers
│ ├─ Services/ # Service classes
│ ├─ Utilities/ # Helper classes (e.g., caching)
│ ├─ Models/ # DTOs and domain models
│ └─ ApiAggregator.csproj
│
├─ ApiAggregator.Tests/ # Unit test project
│ └─ ApiAggregator.Tests.csproj
│
└─ ApiAggregator.sln # Solution file
```

## API Endpoints

## GET `/api/aggregated`

Retrieve aggregated data from all configured external APIs.

## Query Parameters

| Parameter      | Type       | Required | Description |
|----------------|-----------|----------|-------------|
| `searchTerm`   | string    | No       | Filter results by a keyword. If empty, returns all items. |
| `dateOrder`    | string (enum) | No  | Sort results by date. Defaults to `"Descending"`. See [DateSortOrder] for possible values. |
| `dataSources`  | array of strings (enum) | No | Filter results by source. See [DataSource] for possible values. If empty or omitted, results from all sources are returned. |


**Response Example:**
``` json
[
  {
    "source": "NewsApi",
    "title": "Article 1",
    "date": "2025-09-14T12:00:00Z"
  },
  {
    "source": "DevToApi",
    "title": "Post 1",
    "date": "2025-09-14T13:00:00Z"
  }
]
```

## ENUMS
This API uses enums for certain query parameters. These are serialized as strings in JSON.

### DateSortOrder
Used for sorting results by date

| Value          | Description                                  |
| -------------- | -------------------------------------------- |
| `"Ascending"`  | Sort results from oldest to newest           |
| `"Descending"` | Sort results from newest to oldest (default) |

### DataSource
Used for filtering results by source
| Value        | Description                                                                    |
| ------------ | ------------------------------------------------------------------------------ |
| `"GitHub"`   | Data fetched from GitHub API                                                   |
| `"NewsApi"`  | Data fetched from NewsApi. **Note:** only returns articles from the last month |
| `"DevToApi"` | Data fetched from Dev.to API                                                   |

## Caching 
A simple LRU cache (SimpleLruCache) implementation is used to minimize redundant API calls and improve response times. Each Api is assigned its own cache. Cached data is keyed by searchTerm.
When the cache reaches its max capacity, the least recently used (searched in this occasion) entry is discarded.

## Unit Tests
* The project includes unit tests for:
  * Aggregation service (AggregatedService)
  * LRU cache (SimpleLruCache)

## Extensibility
* To add a new API:
  * Implement IExternalApiService.
  * Add the service to AggregatedService constructor list.
  * Update DataSource enum.
  * Incorporate filter logic if needed.