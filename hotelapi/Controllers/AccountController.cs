using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using hotelapi.Models;
using hotelapi.Models.hotelapi.Models;
using hotelapi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace hotelapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IUserService _userService;

        public AccountController(AppDbContext context, IUserService userService)
        {
            this.context = context;
            _userService = userService;
        }

        // GET: api/accounts/GetAccounts
        [HttpGet]
        [Route("GetAccounts")]
        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return await context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .ToListAsync();
        }

        // GET: api/accounts/GetAccountById/5
        [HttpGet]
        [Route("GetAccountById/{id}")]
        public async Task<Account> GetAccountById(int id)
        {
            return await context.Accounts
                .Include(a => a.Customer) 
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == id);
        }

        // POST: api/accounts/AddAccount
        [HttpPost]
        [Route("AddAccount")]
        public async Task<Account> AddAccount([FromBody] Account account)
        {
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
            return account;
        }

        [HttpPut]
        [Route("UpdateAccount/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] Account account)
        {
            if (id != account.AccountId)
            {
                return BadRequest("Account ID mismatch.");
            }

            var existingAccount = await context.Accounts.FindAsync(id);
            if (existingAccount == null)
            {
                return NotFound();
            }

            existingAccount.AccountNumber = account.AccountNumber;
            existingAccount.Balance = account.Balance;
            existingAccount.CustomerId = account.CustomerId;

            await context.SaveChangesAsync();

            return Ok(existingAccount);
        }

        // DELETE: api/accounts/DeleteAccount/5
        [HttpDelete]
        [Route("DeleteAccount/{id}")]
        public async Task<bool> DeleteAccount(int id)
        {
            var account = await context.Accounts.FindAsync(id);
            if (account == null)
                return false;

            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
            return true;
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> SearchAccounts([FromQuery] string? accountNumber, [FromQuery] string? customerName)
        {
            // Log veya debug için:
            Console.WriteLine($"accountNumber: '{accountNumber}', customerName: '{customerName}'");

            var query = context.Accounts
                        .Include(a => a.Customer)
                        .Include(a => a.Transactions)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(accountNumber))
                query = query.Where(a => a.AccountNumber.Contains(accountNumber));

            if (!string.IsNullOrEmpty(customerName))
                query = query.Where(a => a.Customer.Name.Contains(customerName));

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("SearchAccountsAdvanced")] //komple arama sıralama pagging
        public async Task<IEnumerable<Account>> SearchAccountsAdvanced(
        [FromQuery] string accountNumber,
        [FromQuery] string customerName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .AsQueryable();

            if (!string.IsNullOrEmpty(accountNumber))
                query = query.Where(a => a.AccountNumber.Contains(accountNumber));

            if (!string.IsNullOrEmpty(customerName))
                query = query.Where(a => a.Customer.Name.Contains(customerName));

            query = query.OrderBy(a => a.AccountNumber);

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var user = _userService.Authenticate(login.Username, login.Password);

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == registerModel.Username);
            if (existingUser != null)
                return BadRequest("Username already exists.");

            if (registerModel.Password != registerModel.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var user = new User
            {
                Username = registerModel.Username,
                Email = registerModel.Email,
                Password = ComputeSha256Hash(registerModel.Password)  // SHA256 ile hashledik
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }


        public static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }

        [HttpGet]
        [Route("GetAccountCountByCustomer")]
        public async Task<IEnumerable<object>> GetAccountCountByCustomer(decimal? minBalance = null)
        {
            var query = context.Accounts.AsQueryable();

            if (minBalance.HasValue)
                query = query.Where(a => a.Balance >= minBalance.Value);

            var result = await query
                .GroupBy(a => a.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .OrderByDescending(g => g.AccountCount) // azalan hesap sayısına göre sıralama
                .ToListAsync();

            return result;
        }



        [HttpGet]
        [Route("GetCustomerWithMinAccounts")] // en az balance sahip müşteri
        public async Task<IActionResult> GetCustomerWithMinAccounts()
        {
            var grouped = await context.Accounts
                .GroupBy(a => a.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .ToListAsync();

            if (!grouped.Any())
                return NotFound();

            var minCustomer = grouped.OrderBy(r => r.AccountCount).First();

            return Ok(minCustomer);
        }

        [HttpGet]
        [Route("GetCustomerWithMaxAccounts")]// en çok balance sahip müşteri
        public async Task<IActionResult> GetCustomerWithMaxAccounts()
        {
            var grouped = await context.Accounts
                .GroupBy(a => a.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .ToListAsync();

            if (!grouped.Any())
                return NotFound();

            var maxCustomer = grouped.OrderByDescending(r => r.AccountCount).First();

            return Ok(maxCustomer);
        }

        [HttpGet("GetAccountsWithCustomerDetails")]
        public async Task<ActionResult<IEnumerable<AccountWithCustomerDto>>> GetAccountsWithCustomerDetails()
        {
            var accounts = await context.Accounts
                .Include(a => a.Customer)
                .Select(a => new AccountWithCustomerDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    Customer = a.Customer == null ? null : new AccountWithCustomerDto.CustomerInfoDto
                    {
                        CustomerId = a.Customer.CustomerId,
                        Name = a.Customer.Name,
                        Email = a.Customer.Email,
                        Phone = a.Customer.Phone,
                        Address = a.Customer.Address
                    }
                })
                .ToListAsync();

            return Ok(accounts);
        }






    }
}