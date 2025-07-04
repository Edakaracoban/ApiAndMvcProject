using System.Collections.Generic;
using System.Threading.Tasks;
using hotelapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        // GET: api/customer/GetCustomerById/5
        [HttpGet]
        [Route("GetCustomerById/{id}")]
        public async Task<Customer> GetCustomerById(int id)
        {
            return await context.Customers
                .Include(c => c.Accounts)
                .Include(c => c.Loans)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
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

        // Search: api/customer/Search?name=John&email=john@example.com
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<Customer>> Search([FromQuery] string name, [FromQuery] string email)
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

        // GET: api/customer/GetTopCustomersReport //en çok hesaba sahip ilk 5 müşteri raporu
        [HttpGet]
        [Route("GetTopCustomersReport")]
        public async Task<ActionResult<IEnumerable<CustomerReportModel>>> GetTopCustomersReport()
        {
            var result = await context.Accounts
                .Include(a => a.Customer)
                .GroupBy(a => a.Customer.Name)
                .Select(g => new CustomerReportModel
                {
                    CustomerName = g.Key,
                    AccountCount = g.Count(),
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .OrderByDescending(r => r.AccountCount)
                .Take(5)
                .ToListAsync();

            return Ok(result);
        }

    }
}