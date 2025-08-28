using Azure;
using Azure.Data.Tables;

namespace ABCRetailer.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } = "ORDER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int Quantity { get; set; } = 1;
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}