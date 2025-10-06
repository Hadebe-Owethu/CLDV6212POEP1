using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Http;
using ABCRetailer.Models;

namespace ABCRetailer.Services
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public TableNamesOptions TableNames { get; set; } = new();
        public BlobOptions Blob { get; set; } = new();
        public QueueOptions Queue { get; set; } = new();
        public FileShareOptions FileShare { get; set; } = new();
    }
    public class TableNamesOptions
    {
        public string Customers { get; set; } = "Customers";
        public string Products { get; set; } = "Products";
        public string Orders { get; set; } = "Orders";
    }
    public class BlobOptions { public string ContainerName { get; set; } = "productimages"; }
    public class QueueOptions { public string Name { get; set; } = "orders-queue"; }
    public class FileShareOptions { public string Name { get; set; } = "contracts"; }

    public interface IAzureStorageService
    {
        Task InitializeAsync();

        // Table CRUD
        Task AddAsync<T>(T entity) where T : class, ITableEntity, new();
        Task<T?> GetByIdAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();
        Task<List<T>> GetAllAsync<T>() where T : class, ITableEntity, new();
        Task UpsertAsync<T>(T entity) where T : class, ITableEntity, new();
        Task DeleteAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new();

        // Blob
        Task<string?> UploadImageAsync(IFormFile file, string? fileNameHint = null);
        Task DeleteImageAsync(string blobName); // ADD THIS METHOD

        // Queue
        Task EnqueueMessageAsync(string messageText);

        // File Share
        Task UploadContractAsync(string fileName, Stream fileContent);
    }
}