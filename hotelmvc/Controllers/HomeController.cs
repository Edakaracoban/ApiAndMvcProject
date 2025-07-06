using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hotelmvc.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace hotelmvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        private readonly string baseApiUrl = "https://localhost:7013/api/";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new System.Uri(baseApiUrl)
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index()
        {
            var customersResponse = await _httpClient.GetAsync("customer/GetCustomers");
            var customers = JsonConvert.DeserializeObject<List<CustomerModel>>(await customersResponse.Content.ReadAsStringAsync());

            var loansResponse = await _httpClient.GetAsync("loan/GetLoans");
            var loans = JsonConvert.DeserializeObject<List<LoanModel>>(await loansResponse.Content.ReadAsStringAsync());

            var accountsResponse = await _httpClient.GetAsync("account/GetAccounts");
            var accounts = JsonConvert.DeserializeObject<List<AccountModel>>(await accountsResponse.Content.ReadAsStringAsync());

            var transactionsResponse = await _httpClient.GetAsync("transaction/GetTransactions");
            var transactions = JsonConvert.DeserializeObject<List<TransactionModel>>(await transactionsResponse.Content.ReadAsStringAsync());

            // Müşteri bilgilerini hesaplara eşle
            foreach (var account in accounts)
            {
                account.Customer = customers.FirstOrDefault(c => c.CustomerId == account.CustomerId);
            }

            // Genel bilgiler
            var totalAccounts = accounts.Count;
            var totalCustomers = customers.Count;
            var totalBalance = accounts.Sum(a => a.Balance);
            var activeLoans = loans.Count;

            // Kredi statü sayımları
            var approvedLoans = loans.Count(l => l.Status == "Approved");
            var pendingLoans = loans.Count(l => l.Status == "Pending");
            var rejectedLoans = loans.Count(l => l.Status == "Rejected");
            var paidLoans = loans.Count(l => l.Status == "Paid");

            // Diğer bilgiler
            var recentTransactions = transactions.OrderByDescending(t => t.Date).Take(5);
            var topCustomers = customers
                .OrderByDescending(c => c.Accounts?.Count ?? 0)
                .Take(5);

            // ViewModel oluşturuluyor
            var model = new HomeIndexViewModel
            {
                TotalAccounts = totalAccounts,
                TotalCustomers = totalCustomers,
                TotalBalance = totalBalance,
                ActiveLoans = activeLoans,
                Loans = loans,
                RecentTransactions = recentTransactions,
                TopCustomers = topCustomers,
                MonthlyRevenue = 0,
                MonthlyExpense = 0,
                ApprovedLoanCount = approvedLoans,
                PendingLoanCount = pendingLoans,
                RejectedLoanCount = rejectedLoans,
                PaidLoanAccount=paidLoans
            };

            return View(model);
        }



    }
}
