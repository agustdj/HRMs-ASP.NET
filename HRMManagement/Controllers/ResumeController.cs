using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace HRMManagement.Controllers
{
    public class ResumeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ResumeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var client = _clientFactory.CreateClient();
                var content = new MultipartFormDataContent();
                var fileStreamContent = new StreamContent(file.OpenReadStream());
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileStreamContent, "file", file.FileName);

                // Thay đổi URL bằng địa chỉ của API Python của bạn
                var response = await client.PostAsync("http://127.0.0.1:5000/predict", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    // Xử lý kết quả trả về từ API
                    TempData["ClassificationResult"] = result;

                    // Chuyển hướng đến action Result
                    return RedirectToAction("Result", "Resume");
                }
            }
            return RedirectToAction("Result", "Resume");
        }

        public IActionResult Result()
        {
            var classificationResult = TempData["ClassificationResult"] as string;
            ViewBag.ClassificationResult = classificationResult;
            return View();
        }
    }
}
