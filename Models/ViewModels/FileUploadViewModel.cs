using Microsoft.AspNetCore.Http;

namespace ABCRetailer.Models
{
    public class FileUploadViewModel
    {
        public IFormFile? ProofOfPayment { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
    }
}