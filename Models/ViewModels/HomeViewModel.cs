using System.Collections.Generic;

namespace ABCRetailer.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ABCRetailer.Models.Product>? FeaturedProducts { get; set; }
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}
