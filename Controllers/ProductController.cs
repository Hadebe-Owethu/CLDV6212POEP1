using Microsoft.AspNetCore.Mvc;
using ABCRetailer.Models;
using ABCRetailer.Services;

namespace ABCRetailer.Controllers
{
    public class ProductController : Controller
    {
        private readonly IAzureStorageService _storage;

        public ProductController(IAzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Product/
        public async Task<IActionResult> Index()
        {
            var products = await _storage.GetAllAsync<Product>();
            return View(products);
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                product.PartitionKey = "PRODUCT";
                product.RowKey = Guid.NewGuid().ToString();
                product.ProductId = product.RowKey;

                if (imageFile != null)
                {
                    var imageUrl = await _storage.UploadImageAsync(imageFile, product.ProductName);
                    product.ImageUrl = imageUrl;
                }

                await _storage.AddAsync(product);
                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(product);
        }

        // GET: /Product/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _storage.GetByIdAsync<Product>("PRODUCT", id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Product/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    var imageUrl = await _storage.UploadImageAsync(imageFile, product.ProductName);
                    product.ImageUrl = imageUrl;
                }

                await _storage.UpsertAsync(product);
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(product);
        }

        // POST: /Product/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _storage.DeleteAsync<Product>("PRODUCT", id);
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
