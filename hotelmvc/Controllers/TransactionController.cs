using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using hotelmvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace hotelmvc.Controllers
{
    public class TransactionController : Controller
    {
        private readonly string baseApiUrl = "https://localhost:7013/api/Transaction";
        private readonly HttpClient _client;

        public TransactionController()
        {
            _client = new HttpClient();
        }

        public async Task<IActionResult> Index(string? transactionType)
        {
            try
            {
                var url = $"{baseApiUrl}/GetTransactions";
                if (!string.IsNullOrEmpty(transactionType))
                {
                    url += $"?transactionType={transactionType}";
                }

                var transactions = await _client.GetFromJsonAsync<List<TransactionModel>>(url);

                ViewBag.TransactionType = transactionType; // Arama kutusuna geri doldurmak için
                return View(transactions);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Veriler alınırken hata oluştu: " + ex.Message;
                return View(new List<TransactionModel>());
            }
        }


        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var transaction = await _client.GetFromJsonAsync<TransactionModel>($"{baseApiUrl}/GetTransactionById/{id}");
                if (transaction == null)
                    return NotFound();

                return View(transaction);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Veri alınırken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Transaction/Create
        public async Task<IActionResult> Create()
        {
            var accounts = await _client.GetFromJsonAsync<List<AccountModel>>("https://localhost:7013/api/Account/GetAccounts");
            ViewBag.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"#{a.AccountId} - {a.Customer?.Name ?? "No Name"}"
            }).ToList();

            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionModel transaction)
        {
            if (!ModelState.IsValid)
                return View(transaction);

            var response = await _client.PostAsJsonAsync($"{baseApiUrl}/AddTransaction", transaction);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Transaction eklenirken hata oluştu.");
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _client.GetFromJsonAsync<TransactionModel>($"{baseApiUrl}/GetTransactionById/{id}");
            if (transaction == null) return NotFound();

            var accounts = await _client.GetFromJsonAsync<List<AccountModel>>("https://localhost:7013/api/Account/GetAccounts");
            ViewBag.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"#{a.AccountId} - {a.Customer?.Name ?? "No Name"}"
            }).ToList();

            return View(transaction);
        }


        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransactionModel transaction)
        {
            if (id != transaction.TransactionId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(transaction);

            var response = await _client.PutAsJsonAsync($"{baseApiUrl}/UpdateTransaction/{id}", transaction);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Transaction güncellenirken hata oluştu.");
            return View(transaction);
        }
        // Silme
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"{baseApiUrl}/DeleteTransaction/{id}");
            return RedirectToAction("Index");
        }

       

        // GET: Transaction/Stats
        public async Task<IActionResult> Stats()
        {
            try
            {
                var stats = await _client.GetFromJsonAsync<List<TransactionStatsModel>>($"{baseApiUrl}/GetTransactionStatsByType");
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "İstatistikler alınırken hata oluştu: " + ex.Message;
                return View(new List<TransactionStatsModel>());
            }
        }
    }
}
