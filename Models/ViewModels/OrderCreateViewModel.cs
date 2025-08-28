using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ABCRetailer.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public IEnumerable<SelectListItem>? Customers { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }
    }
}