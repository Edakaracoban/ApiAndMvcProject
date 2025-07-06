using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hotelmvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text.Json;

namespace hotelmvc.Controllers
{
    public class LoanController : Controller
    {
        private readonly string baseApiUrl = "https://localhost:7013/api/Loan";
        private readonly HttpClient _client;

        public LoanController()
        {
            _client = new HttpClient();
        }
        public async Task<IActionResult> Index(string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            string url;

            // Arama kriteri varsa Search endpoint'i kullanılmalı
            if (!string.IsNullOrEmpty(status) || startDate.HasValue || endDate.HasValue)
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(status))
                    queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

                url = $"{baseApiUrl}/Search";
                if (queryParams.Any())
                    url += "?" + string.Join("&", queryParams);
            }
            else
            {
                // Filtre yoksa tüm kredileri getir
                url = $"{baseApiUrl}/GetLoans";
            }

            try
            {
                var loans = await _client.GetFromJsonAsync<List<LoanModel>>(url);

                ViewData["Status"] = status;
                ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

                return View(loans);
            }
            catch (HttpRequestException ex)
            {
                // Hata durumunda loglama yapabilir veya ViewBag ile kullanıcıya bilgi verebilirsin
                ViewBag.Error = "Veri alınırken bir hata oluştu: " + ex.Message;
                return View(new List<LoanModel>());
            }
        }



        public async Task<IActionResult> Details(int id)
        {
            var loan = await _client.GetFromJsonAsync<LoanModel>($"{baseApiUrl}/GetLoanById/{id}");
            if (loan == null) return NotFound();
            return View(loan);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCustomersAsync();
            return View();
        }

        // POST: Loan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanModel loan)
        {
            await LoadCustomersAsync(); // ViewBag doldurulmalı

            if (!ModelState.IsValid)
            {
                return View(loan);
            }

            var response = await _client.PostAsJsonAsync($"{baseApiUrl}/AddLoan", loan);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kredi eklenirken hata oluştu.");
            return View(loan);
        }

        // Ortak kullanılan müşteri yükleme metodu
        private async Task LoadCustomersAsync()
        {
            var customerResponse = await _client.GetAsync($"{baseApiUrl.Replace("Loan", "Customer")}/GetCustomers");

            List<CustomerModel> customers = new();
            if (customerResponse.IsSuccessStatusCode)
            {
                var customersJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersJson);
            }

            ViewBag.Customers = new SelectList(customers, "CustomerId", "Name");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var loan = await _client.GetFromJsonAsync<LoanModel>($"{baseApiUrl}/GetLoanById/{id}");
            if (loan == null) return NotFound();
            return View(loan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoanModel loan)
        {
            if (id != loan.LoanId) return BadRequest();

            if (!ModelState.IsValid) return View(loan);

            var response = await _client.PutAsJsonAsync($"{baseApiUrl}/UpdateLoan/{id}", loan);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kredi güncellenirken hata oluştu.");
            return View(loan);
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"{baseApiUrl}/DeleteLoan/{id}");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Stats()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = await _client.GetAsync($"{baseApiUrl}/GetLoanStatsByStatus");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"API çağrısı başarısız: {response.StatusCode}";
                return View(new List<LoanStatModel>());
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var stats = System.Text.Json.JsonSerializer.Deserialize<List<LoanStatModel>>(jsonString, options);

            if (stats == null || !stats.Any())
            {
                TempData["Error"] = "API’den istatistik verisi alınamadı veya boş döndü.";
            }

            return View(stats);
        }
        public async Task<IActionResult> StatsByMonth()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = await _client.GetAsync($"{baseApiUrl}/GetLoanStatsByMonth");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"API çağrısı başarısız: {response.StatusCode}";
                return View(new List<LoanStatByMonthModel>());
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            var stats = System.Text.Json.JsonSerializer.Deserialize<List<LoanStatByMonthModel>>(jsonString, options);

            if (stats == null || !stats.Any())
            {
                TempData["Error"] = "API’den istatistik verisi alınamadı veya boş döndü.";
            }

            return View(stats);
        }



    }
}
