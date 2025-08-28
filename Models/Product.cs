using Azure;
using Azure.Data.Tables;

namespace ABCRetailer.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductId { get; set; } = Guid.NewGuid().ToString();
        public string ProductName { get; set; }     
        public string Description { get; set; }     
        public double Price { get; set; }          
        public int StockAvailable { get; set; }     
        public string? ImageUrl { get; set; }        
    }
}
