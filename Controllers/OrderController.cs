using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ABCRetailer.Services;
using ABCRetailer.Models;
using ABCRetailer.Models.ViewModels;

namespace ABCRetailer.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAzureStorageService _storage;
        public OrderController(IAzureStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var orders = await _storage.GetAllAsync<Order>();
            return View(orders.OrderByDescending(o => o.OrderDate));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customers = await _storage.GetAllAsync<Customer>();
            var products = await _storage.GetAllAsync<Product>();

            var vm = new OrderCreateViewModel
            {
                Customers = customers.Select(c => new SelectListItem($"{c.Name} {c.Surname}", c.CustomerId)),
                Products = products.Select(p => new SelectListItem(p.ProductName, p.ProductId))
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateViewModel vm)
        {
            var products = await _storage.GetAllAsync<Product>();
            var product = products.First(p => p.ProductId == vm.ProductId);
            var order = new Order
            {
                CustomerId = vm.CustomerId ?? "",
                ProductId = vm.ProductId ?? "",
                ProductName = product.ProductName,
                Quantity = vm.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * vm.Quantity,
                Status = vm.Status
            };
            await _storage.UpsertAsync(order);
            await _storage.EnqueueMessageAsync($"New order placed: {order.OrderId} for {order.TotalPrice}");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var orders = await _storage.GetAllAsync<Order>();
            var entity = orders.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);
            if (entity == null) return NotFound();
            return View(entity);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Order model)
        {
            await _storage.UpsertAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var orders = await _storage.GetAllAsync<Order>();
            var entity = orders.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _storage.DeleteAsync<Order>(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}