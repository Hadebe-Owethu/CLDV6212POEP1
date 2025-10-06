using Azure;
using Azure.Data.Tables;
using System;
using Microsoft.AspNetCore.Http;

namespace ABCRetailer.Models
{
    public class FileUploadModel : ITableEntity
    {
        public string PartitionKey { get; set; } = "UPLOAD";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // MVC Form Property (for file upload in MVC)
        public IFormFile? ProofOfPayment { get; set; }

        // Table Storage Properties
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string FileName { get; set; }
        public string BlobUrl { get; set; }

        // ADD THESE MISSING PROPERTIES:
        public string ContentType { get; set; }
        public string Status { get; set; } = "Pending";

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}