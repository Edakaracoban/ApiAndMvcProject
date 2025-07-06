using System.Collections.Generic;
using System.Threading.Tasks;
using hotelapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using hotelmvc.Models;
using CustomerReportModel = hotelmvc.Models.CustomerReportModel;

namespace hotelapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext context;

        public CustomerController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/customer/GetCustomers
        [HttpGet]
        [Route("GetCustomers")]
        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            return await context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .ToListAsync();
        }

        [HttpGet("GetCustomerById/{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await context.Customers
                                         .Include(c => c.Accounts)  // Hesaplar dahil
                                         .Include(c => c.Loans)     // Krediler dahil
                                         .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            return Ok(customer);
        }
        // POST: api/customer/AddCustomer
        [HttpPost]
        [Route("AddCustomer")]
        public async Task<Customer> AddCustomer([FromBody] Customer customer)
        {
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            return customer;
        }

        // PUT: api/customer/UpdateCustomer/5
        [HttpPut]
        [Route("UpdateCustomer/{id}")]
        public async Task<Customer> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            // Güncelleme için id kontrolü yapabilirsin (isteğe bağlı)
            if (id != customer.CustomerId)
            {
                // İstersen hata döndürebilirsin
                return null;
            }

            context.Customers.Update(customer);
            await context.SaveChangesAsync();
            return customer;
        }

        // DELETE: api/customer/DeleteCustomer/5
        [HttpDelete]
        [Route("DeleteCustomer/{id}")]
        public async Task<bool> DeleteCustomer(int id)
        {
            var customer = await context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return true;
        }
        [HttpGet("Search")]
        public async Task<IEnumerable<Customer>> Search([FromQuery] string? name = null, [FromQuery] string? email = null)
        {
            var query = context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email.Contains(email));

            return await query.ToListAsync();
        }

        // Grup ve sayma örneği: Müşteri adına göre hesap sayısı ve toplam bakiye
        // GET: api/customer/GetAccountStatsByCustomer
        [HttpGet]
        [Route("GetAccountStatsByCustomer")]
        public async Task<IEnumerable<object>> GetAccountStatsByCustomer()
        {
            var result = await context.Accounts
                .Include(a => a.Customer)
                .GroupBy(a => a.Customer.Name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .ToListAsync();

            return result;
        }

        // Gelişmiş arama, sıralama ve paging
        // GET: api/customer/SearchAdvanced?name=John&page=1&pageSize=10
        [HttpGet]
        [Route("SearchAdvanced")]
        public async Task<IEnumerable<Customer>> SearchAdvanced(
            [FromQuery] string name,
            [FromQuery] string email,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email.Contains(email));

            query = query.OrderBy(c => c.Name);

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        [HttpGet("GetTopCustomersReport")]
        public async Task<IEnumerable<object>> GetTopCustomersReport()
        {
            var topCustomers = await context.Accounts
                .Include(a => a.Customer)
                .GroupBy(a => new { a.Customer.CustomerId, a.Customer.Name })
                .Select(g => new
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.Name,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .OrderByDescending(g => g.AccountCount)
                .Take(5)
                .ToListAsync();

            return topCustomers;
        }
        [HttpGet("GetCustomersByAddress")]
        public async Task<IEnumerable<Models.AddressGroupModel>> GetCustomersByAddress()
        {
            var result = await context.Customers
                .Include(c => c.Accounts)
                .GroupBy(c => c.Address)
                .Select(g => new Models.AddressGroupModel
                {
                    Address = g.Key,
                    CustomerCount = g.Count(),
                    TotalBalance = g.Sum(c => c.Accounts.Sum(a => a.Balance))
                })
                .ToListAsync();

            return result;
        }
        [HttpGet("GetLoanCustomerDetails")]
        public async Task<IEnumerable<object>> GetLoanCustomerDetails()
        {
            var result = await context.Loans
                .Include(l => l.Customer)
                .Select(l => new
                {
                    LoanId = l.LoanId,
                    LoanAmount = l.LoanAmount,
                    Status = l.Status,
                    CustomerId = l.Customer.CustomerId,
                    CustomerName = l.Customer.Name,
                    CustomerEmail = l.Customer.Email
                })
                .ToListAsync();

            return result;
        }
        [HttpGet("GetCustomersSortedAsc")]
        public async Task<IEnumerable<Customer>> GetCustomersSortedAsc()
        {
            return await context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

    }
}