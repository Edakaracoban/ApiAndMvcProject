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
    public class LoanController : ControllerBase
    {
        private readonly AppDbContext context;

        public LoanController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/loan/GetLoans
        [HttpGet]
        [Route("GetLoans")]
        public async Task<IEnumerable<Loan>> GetLoans()
        {
            return await context.Loans
                .Include(l => l.Customer)
                .ToListAsync();
        }

        // GET: api/loan/GetLoanById/5
        [HttpGet]
        [Route("GetLoanById/{id}")]
        public async Task<Loan> GetLoanById(int id)
        {
            return await context.Loans
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LoanId == id);
        }

        // POST: api/loan/AddLoan
        [HttpPost]
        [Route("AddLoan")]
        public async Task<Loan> AddLoan([FromBody] Loan loan)
        {
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            return loan;
        }

        // PUT: api/loan/UpdateLoan/5
        [HttpPut]
        [Route("UpdateLoan/{id}")]
        public async Task<Loan> UpdateLoan(int id, [FromBody] Loan loan)
        {
            if (id != loan.LoanId)
                return null; // veya BadRequest dön

            context.Loans.Update(loan);
            await context.SaveChangesAsync();
            return loan;
        }

        // DELETE: api/loan/DeleteLoan/5
        [HttpDelete]
        [Route("DeleteLoan/{id}")]
        public async Task<bool> DeleteLoan(int id)
        {
            var loan = await context.Loans.FindAsync(id);
            if (loan == null)
                return false;

            context.Loans.Remove(loan);
            await context.SaveChangesAsync();
            return true;
        }

        // Arama: Status ve Tarih aralığına göre filtreleme
        // GET: api/loan/Search?status=Approved&startDate=2023-01-01&endDate=2023-12-31
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<Loan>> Search(
            [FromQuery] string status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var query = context.Loans
                .Include(l => l.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status.Contains(status));

            if (startDate.HasValue)
                query = query.Where(l => l.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.EndDate <= endDate.Value);

            return await query.ToListAsync();
        }

        // Örnek grup sorgusu: Duruma göre kredi sayısı ve toplam kredi miktarı
        // GET: api/loan/GetLoanStatsByStatus
        [HttpGet]
        [Route("GetLoanStatsByStatus")]
        public async Task<IEnumerable<object>> GetLoanStatsByStatus()
        {
            var result = await context.Loans
                .GroupBy(l => l.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    LoanCount = g.Count(),
                    TotalLoanAmount = g.Sum(l => l.LoanAmount)
                })
                .ToListAsync();

            return result;
        }
    }
}