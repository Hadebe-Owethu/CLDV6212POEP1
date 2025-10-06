using Microsoft.AspNetCore.Mvc;
using ABCRetailer.Models;
using System.Text;
using System.Text.Json;

namespace ABCRetailer.Controllers
{
    public class UploadController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;

        public UploadController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _functionBaseUrl = configuration["AzureFunctions:BaseUrl"] ?? "http://localhost:7071/api";
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new FileUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadViewModel model)
        {
            if (model.ProofOfPayment == null || model.ProofOfPayment.Length == 0)
            {
                ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.CustomerName) || string.IsNullOrEmpty(model.OrderId))
            {
                ModelState.AddModelError("", "Customer Name and Order ID are required.");
                return View(model);
            }

            try
            {
                // Create multipart form data for Azure Function
                using var formData = new MultipartFormDataContent();

                // Add file
                using var fileStream = model.ProofOfPayment.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ProofOfPayment.ContentType);
                formData.Add(fileContent, "file", model.ProofOfPayment.FileName);

                // Add form fields
                formData.Add(new StringContent(model.CustomerName), "customerName");
                formData.Add(new StringContent(model.OrderId), "orderId");

                // Call Azure Function upload endpoint
                var response = await _httpClient.PostAsync($"{_functionBaseUrl}/upload", formData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    TempData["Message"] = $"File '{model.ProofOfPayment.FileName}' uploaded successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Upload failed: {error}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                // Call Azure Function to get uploads list
                var response = await _httpClient.GetAsync($"{_functionBaseUrl}/uploads");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var uploads = JsonSerializer.Deserialize<List<FileUploadModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return View(uploads ?? new List<FileUploadModel>());
                }
                else
                {
                    TempData["Error"] = $"Failed to retrieve uploads: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error retrieving uploads: {ex.Message}";
            }

            return View(new List<FileUploadModel>());
        }
    }
}