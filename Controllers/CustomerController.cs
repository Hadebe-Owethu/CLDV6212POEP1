using Microsoft.AspNetCore.Mvc;
using ABCRetailer.Models;
using ABCRetailer.Services;

namespace ABCRetailer.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storage;

        public CustomerController(IAzureStorageService storage)
        {
            _storage = storage;
        }

        // GET: /Customer/
        public async Task<IActionResult> Index() 
        { 
            var customers = await _storage.GetAllAsync<Customer>(); 
            return View(customers); 
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Assign keys if not already set
                customer.PartitionKey = "CUSTOMER";
                customer.RowKey = Guid.NewGuid().ToString();
                customer.CustomerId = customer.RowKey;

                await _storage.AddAsync(customer);
                TempData["SuccessMessage"] = "Customer created successfully.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(customer);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var customer = await _storage.GetByIdAsync<Customer>("CUSTOMER", id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _storage.UpsertAsync(customer);
                TempData["SuccessMessage"] = "Customer updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Please correct the errors and try again.";
            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _storage.DeleteAsync<Customer>("CUSTOMER", id);
            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToAction("Index");
        }

    }
}
