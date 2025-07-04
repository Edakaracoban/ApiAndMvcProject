using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using hotelmvc.Models;
using System.Text;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace hotelmvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly string baseApiUrl = "https://localhost:7013/api/Account";
        private readonly HttpClient _client;

        public AccountController()
        {
            _client = new HttpClient();
        }

        public async Task<IActionResult> Index(string accountNumber, string customerName)
        {
            string url = $"{baseApiUrl}/Search?";

            if (!string.IsNullOrEmpty(accountNumber))
                url += $"accountNumber={accountNumber}&";

            if (!string.IsNullOrEmpty(customerName))
                url += $"customerName={customerName}&";

            var response = await _client.GetAsync(url);
            List<AccountModel> accounts = new List<AccountModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                accounts = JsonConvert.DeserializeObject<List<AccountModel>>(json);
            }
            ViewData["AccountNumber"] = accountNumber;
            ViewData["CustomerName"] = customerName;
            return View(accounts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetAccountById/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountModel>(json);
            return View(account);
        }

        public async Task<IActionResult> Create()
        {

            var customerResponse=await _client.GetAsync($"{baseApiUrl.Replace("Account", "Customer")}/GetCustomers");
            List<CustomerModel> customers = new List<CustomerModel>();
            if (customerResponse.IsSuccessStatusCode)
            {
                var customersJson = await customerResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersJson);
            }
            ViewBag.Customers = new SelectList(customers, "CustomerId", "Name");

            return View(new AccountModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountModel model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseApiUrl}/AddAccount", content);
            var customersResponse = await _client.GetAsync($"{baseApiUrl.Replace("Account", "Customer")}/GetCustomers");
            List<CustomerModel> customers = new List<CustomerModel>();

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Failed to create account");
            ViewBag.Customers = new SelectList(customers, "CustomerId", "Name", model.CustomerId);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetAccountById/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<AccountModel>(json);

            var customersResponse = await _client.GetAsync($"{baseApiUrl.Replace("Account", "Customer")}/GetCustomers");
            List<CustomerModel> customers = new List<CustomerModel>();
            if (customersResponse.IsSuccessStatusCode)
            {
                var customersJson = await customersResponse.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersJson);
            }

            ViewBag.Customers = new SelectList(customers, "CustomerId", "Name", account.CustomerId);

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AccountModel model)
        {
            System.Diagnostics.Debug.WriteLine($"CustomerId from form: {model.CustomerId}");
            if (!ModelState.IsValid)
            {
                // Model geçersizse müşteri listesini tekrar yükle
                var customersResponse = await _client.GetAsync("https://localhost:7013/api/Customer/GetCustomers");
                List<CustomerModel> customers = new List<CustomerModel>();
                if (customersResponse.IsSuccessStatusCode)
                {
                    var customersJson = await customersResponse.Content.ReadAsStringAsync();
                    customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersJson);
                }

                ViewBag.Customers = new SelectList(customers, "CustomerId", "Name", model.CustomerId);

                return View(model);
            }

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"{baseApiUrl}/UpdateAccount/{model.AccountId}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Account");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to update account: {errorContent}");

                // Hata durumunda da müşteri listesini tekrar yükle
                var customersResponse = await _client.GetAsync($"{baseApiUrl.Replace("Account", "Customer")}/GetCustomers");
                List<CustomerModel> customers = new List<CustomerModel>();
                if (customersResponse.IsSuccessStatusCode)
                {
                    var customersJson = await customersResponse.Content.ReadAsStringAsync();
                    customers = JsonConvert.DeserializeObject<List<CustomerModel>>(customersJson);
                }

                ViewBag.Customers = new SelectList(customers, "CustomerId", "Name", model.CustomerId);

                return View(model);
            }
        }



        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"{baseApiUrl}/DeleteAccount/{id}");
            return RedirectToAction("Index");
        }

  
        public async Task<IActionResult> AdvancedSearch(string accountNumber, string customerName, int page = 1, int pageSize = 10)
        {
            string url = $"{baseApiUrl}/SearchAccountsAdvanced?accountNumber={accountNumber}&customerName={customerName}&page={page}&pageSize={pageSize}";

            var response = await _client.GetAsync(url);
            List<AccountModel> accounts = new List<AccountModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                accounts = JsonConvert.DeserializeObject<List<AccountModel>>(json);
            }

            return View(accounts);
        }

        // Giriş sayfasını gösterir
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        // Giriş işlemi yapar
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseApiUrl}/Login", content);

            if (response.IsSuccessStatusCode)
            {
                // Giriş başarılı, token veya kullanıcı bilgilerini al
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(jsonResponse);

                // Kullanıcı bilgilerini session veya cookie'ye kaydet (örnek session)
                HttpContext.Session.SetString("Username", user.Username);
                // İstersen Authentication Cookie ile de yapabilirsin

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View(model);
            }
        }
        // Kayıt sayfasını gösterir
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        // Kayıt işlemini yapar
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{baseApiUrl}/Register", content);

            if (response.IsSuccessStatusCode)
            {
                // Kayıt başarılı, kullanıcıyı giriş sayfasına yönlendir
                return RedirectToAction("Login");
            }
            else
            {
                // Hata varsa mesaj göster
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Kayıt işlemi başarısız: " + errorMsg);
                return View(model);
            }
        }
        // Şifre unutma sayfasını gösterir (email girişi)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Lütfen e-posta adresinizi giriniz.");
                return View();
            }

            var resetLink = Url.Action("ResetPassword", "Account", new { email = email, token = "tokenDeğeri" }, Request.Scheme);

            string subject = "Şifre Sıfırlama Talebi";
            string body = $"Şifrenizi sıfırlamak için lütfen <a href='{resetLink}'>buraya tıklayın</a>.";

            bool mailSent = await SendEmailAsync(email, subject, body);

            if (mailSent)
            {
                ViewBag.Message = "Şifre sıfırlama linki e-posta adresinize gönderildi.";
                return View();
            }
            else
            {
                // ModelState zaten SendEmailAsync içinde güncelleniyor, ama tekrar ekleyebilirsin
                if (!ModelState.ContainsKey(""))
                    ModelState.AddModelError("", "E-posta gönderilirken hata oluştu.");
                return View();
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("your-email@gmail.com", "Your App Name"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Hata mesajını debug'a yazdır
                System.Diagnostics.Debug.WriteLine($"E-posta gönderim hatası: {ex.Message}");

                // Opsiyonel: ModelState veya ViewData ile kullanıcıya göstermek için
                ModelState.AddModelError("", $"E-posta gönderilirken hata oluştu: {ex.Message}");

                return false;
            }
        }
        public async Task<IActionResult> AccountSummary(decimal? minBalance)
        {
            var url = $"{baseApiUrl}/GetAccountCountByCustomer";

            if (minBalance.HasValue)
                url += $"?minBalance={minBalance.Value}";

            var response = await _client.GetAsync(url);
            List<AccountSummaryViewModel> summary = new List<AccountSummaryViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                summary = JsonConvert.DeserializeObject<List<AccountSummaryViewModel>>(json);
            }

            ViewBag.MinBalance = minBalance;

            return View(summary);
        }

        public async Task<IActionResult> CustomerWithMinAccounts()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomerWithMinAccounts");
            if (!response.IsSuccessStatusCode)
                return View(null);

            var json = await response.Content.ReadAsStringAsync();
            var minCustomer = JsonConvert.DeserializeObject<CustomerAccountSummaryViewModel>(json);

            return View(minCustomer);
        }

        // En çok hesap sayısına sahip müşteri
        public async Task<IActionResult> CustomerWithMaxAccounts()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetCustomerWithMaxAccounts");
            if (!response.IsSuccessStatusCode)
                return View(null);

            var json = await response.Content.ReadAsStringAsync();
            var maxCustomer = JsonConvert.DeserializeObject<CustomerAccountSummaryViewModel>(json);

            return View(maxCustomer);
        }
        public async Task<IActionResult> GetAccountsWithCustomerDetails()
        {
            var response = await _client.GetAsync($"{baseApiUrl}/GetAccountsWithCustomerDetails");
            if (!response.IsSuccessStatusCode)
                return View(null);

            var json = await response.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<AccountWithCustomerDto>>(json);

            return View(accounts);
        }










    }
}