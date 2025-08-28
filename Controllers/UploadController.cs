using Microsoft.AspNetCore.Mvc;
using ABCRetailer.Models;

namespace ABCRetailer.Controllers
{
    public class UploadController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (model.ProofOfPayment == null || model.ProofOfPayment.Length == 0)
            {
                ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                return View(model);
            }

            // Simulate file handling
            var fileName = Path.GetFileName(model.ProofOfPayment.FileName);
            var filePath = Path.Combine("wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProofOfPayment.CopyToAsync(stream);
            }

            TempData["Message"] = $"File '{fileName}' uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
