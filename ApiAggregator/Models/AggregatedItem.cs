namespace ApiAggregator.Models
{
    public enum DateSortOrder
    {
        Ascending,
        Descending
    }

    public enum DataSource
    {
        GitHub,
        NewsApi,
        DevToApi
    }


    public class AggregatedItem
    {
        public string Source { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime? Date { get; set; }
    }
}
