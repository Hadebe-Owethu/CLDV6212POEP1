using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text;
using ABCRetailer.Models;

namespace ABCRetailer.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly AzureStorageOptions _options;
        private readonly TableServiceClient _tableServiceClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly QueueClient _queueClient;
        private readonly ShareClient _shareClient;

        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;
        private readonly TableClient _orderTable;
        private readonly BlobContainerClient _blobContainer;

        public AzureStorageService(IOptions<AzureStorageOptions> options)
        {
            _options = options.Value;

            _tableServiceClient = new TableServiceClient(_options.ConnectionString);
            _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            _queueClient = new QueueClient(_options.ConnectionString, _options.Queue.Name);
            _shareClient = new ShareClient(_options.ConnectionString, _options.FileShare.Name);

            _customerTable = _tableServiceClient.GetTableClient(_options.TableNames.Customers);
            _productTable = _tableServiceClient.GetTableClient(_options.TableNames.Products);
            _orderTable = _tableServiceClient.GetTableClient(_options.TableNames.Orders);
            _blobContainer = _blobServiceClient.GetBlobContainerClient(_options.Blob.ContainerName);

            // Auto-initialize (for development)
            InitializeAsync().GetAwaiter().GetResult();
        }

        public async Task InitializeAsync()
        {
            await _customerTable.CreateIfNotExistsAsync();
            await _productTable.CreateIfNotExistsAsync();
            await _orderTable.CreateIfNotExistsAsync();

            await _blobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);
            await _queueClient.CreateIfNotExistsAsync();
            await _shareClient.CreateIfNotExistsAsync();
        }

        // Table Operations
        public async Task<List<T>> GetAllAsync<T>() where T : class, ITableEntity, new()
        {
            var table = GetTableFor<T>();
            var results = new List<T>();
            await foreach (var entity in table.QueryAsync<T>())
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task<T?> GetByIdAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var table = GetTableFor<T>();
            try
            {
                var response = await table.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpsertAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var table = GetTableFor<T>();
            await table.UpsertEntityAsync(entity);
        }

        public async Task AddAsync<T>(T entity) where T : class, ITableEntity, new()
        {
            var table = GetTableFor<T>();
            await table.AddEntityAsync(entity);
        }

        public async Task DeleteAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var table = GetTableFor<T>();
            await table.DeleteEntityAsync(partitionKey, rowKey);
        }

        // Blob Operations
        public async Task<string?> UploadImageAsync(IFormFile file, string? fileNameHint = null)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = (fileNameHint ?? Guid.NewGuid().ToString()) + Path.GetExtension(file.FileName);
            var blobClient = _blobContainer.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string blobName)
        {
            var blobClient = _blobContainer.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        // Queue Operations
        public async Task EnqueueMessageAsync(string messageText)
        {
            var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageText));
            await _queueClient.SendMessageAsync(encodedMessage);
        }

        // File Share Operations
        public async Task UploadContractAsync(string fileName, Stream fileContent)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);
            fileContent.Position = 0;

            await fileClient.CreateAsync(fileContent.Length);
            await fileClient.UploadAsync(fileContent);
        }

        // Internal Table Mapper
        private TableClient GetTableFor<T>() where T : class, ITableEntity, new()
        {
            return typeof(T) switch
            {
                var t when t == typeof(Customer) => _customerTable,
                var t when t == typeof(Product) => _productTable,
                var t when t == typeof(Order) => _orderTable,
                var t when t == typeof(ABCRetailer.Models.FileUploadModel) => _customerTable, // Use customers table or create separate
                _ => throw new InvalidOperationException($"No table mapping defined for type {typeof(T).Name}")
            };
        }


    }
}
