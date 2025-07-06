using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using hotelmvc.Models;
using System.Text;

namespace hotelmvc.Controllers
{
    public class CustomerController : Controller
    {
        private readonly string baseApiUrl = "https://localhost:7013/api/Customer";
        private readonly HttpClient _client;

        public CustomerController()
        {
            _client = new HttpClient();
        }

        // Listeleme + basit arama
        public async Task<IActionResult> Index(string name, string email)
        {
            string url = $"{baseApiUrl}/Search?";

            if (!string.IsNullOrEmpty(name))
                url += $"name={Uri.EscapeDataString(name)}&";

            if (!string.IsNullOrEmpty(email))
                url += $"email={Uri.EscapeDataString(email)}&";

            var response = await _client.GetAsync(url);
            var customers = new List<CustomerModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(json);
            }

            ViewData["Name"] = name;
            ViewData["Email"] = email;
            return View(customers);
        }

        // Detay görüntüleme
        public async Task<IActionResult> Details(int id)
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomerById/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerModel>(json);
            return View(customer);
        }

        // Oluşturma — GET
        public IActionResult Create()
        {
            return View(new CustomerModel());
        }

        // Oluşturma — POST
        [HttpPost]
        public async Task<IActionResult> Create(CustomerModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseApiUrl}/AddCustomer", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Müşteri oluşturulamadı: {errorContent}");
            return View(model);
        }

        // Düzenleme — GET
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomerById/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CustomerModel>(json);
            return View(model);
        }

        // Düzenleme — POST
        [HttpPost]
        public async Task<IActionResult> Edit(CustomerModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseApiUrl}/UpdateCustomer/{model.CustomerId}", content);
            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Güncelleme başarısız: {error}");
            return View(model);
        }

        // Silme
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"{baseApiUrl}/DeleteCustomer/{id}");
            return RedirectToAction("Index");
        }

        // İstatistik: müşteri başına hesap sayısı + bakiye
        public async Task<IActionResult> GetAccountStatsByCustomer()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetAccountStatsByCustomer");
            var stats = new List<AccountSummaryModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                stats = JsonConvert.DeserializeObject<List<AccountSummaryModel>>(json);
            }
            return View(stats);
        }

        // Gelişmiş arama / paging
        public async Task<IActionResult> SearchAdvanced(string name, string email, int page = 1, int pageSize = 10)
        {
            string url = $"{baseApiUrl}/SearchAdvanced?name={name}&email={email}&page={page}&pageSize={pageSize}";
            var response = await _client.GetAsync(url);
            var list = new List<CustomerModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<CustomerModel>>(json);
            }

            ViewData["Name"] = name;
            ViewData["Email"] = email;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            return View(list);
        }

        // Rapor: en çok hesaba sahip ilk 5 müşteri
        public async Task<IActionResult> CustomerReport()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetTopCustomersReport");
            var report = new List<CustomerReportModel>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                report = JsonConvert.DeserializeObject<List<CustomerReportModel>>(json);
            }

            return View(report);
        }

        // Adrese göre gruplama
        public async Task<IActionResult> CustomersByAddress()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomersByAddress");
            var list = new List<AddressGroupModel>();
            if (response.IsSuccessStatusCode)
                list = JsonConvert.DeserializeObject<List<AddressGroupModel>>(await response.Content.ReadAsStringAsync());
            return View(list);
        }

        // Loan + Customer detaylar
        public async Task<IActionResult> GetLoanCustomerDetails()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetLoanCustomerDetails");
            var list = new List<LoanCustomerDetailsModel>();
            if (response.IsSuccessStatusCode)
                list = JsonConvert.DeserializeObject<List<LoanCustomerDetailsModel>>(await response.Content.ReadAsStringAsync());
            return View(list);
        }

        // A–Z sıralı list
        public async Task<IActionResult> SortedAsc()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomersSortedAsc");
            var list = new List<CustomerModel>();
            if (response.IsSuccessStatusCode)
                list = JsonConvert.DeserializeObject<List<CustomerModel>>(await response.Content.ReadAsStringAsync());
            return View("Index", list);
        }
    }
}
