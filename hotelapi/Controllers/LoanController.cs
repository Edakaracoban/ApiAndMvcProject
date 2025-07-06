using System;
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
        [HttpGet("GetLoans")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            var loans = await context.Loans
                .Include(l => l.Customer)
                .ToListAsync();
            return Ok(loans);
        }

        // GET: api/loan/GetLoanById/5
        [HttpGet("GetLoanById/{id}")]
        public async Task<ActionResult<Loan>> GetLoanById(int id)
        {
            var loan = await context.Loans
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound();

            return Ok(loan);
        }

        // POST: api/loan/AddLoan
        [HttpPost("AddLoan")]
        public async Task<ActionResult<Loan>> AddLoan([FromBody] Loan loan)
        {
            if (loan == null)
                return BadRequest();

            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoanById), new { id = loan.LoanId }, loan);
        }

        // PUT: api/loan/UpdateLoan/5
        [HttpPut("UpdateLoan/{id}")]
        public async Task<ActionResult<Loan>> UpdateLoan(int id, [FromBody] Loan loan)
        {
            if (loan == null || id != loan.LoanId)
                return BadRequest();

            var existingLoan = await context.Loans.FindAsync(id);
            if (existingLoan == null)
                return NotFound();
            context.Entry(existingLoan).CurrentValues.SetValues(loan);

            await context.SaveChangesAsync();

            return Ok(loan);
        }
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
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Loan>>> Search(
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

            var result = await query.ToListAsync();
            return Ok(result);
        }



        [HttpGet("GetLoanStatsByStatus")]
        public async Task<ActionResult<IEnumerable<LoanStatModel>>> GetLoanStatsByStatus()
        {
            var result = await context.Loans
                .GroupBy(l => l.Status)
                .Select(g => new LoanStatModel
                {
                    Status = g.Key,
                    LoanCount = g.Count(),
                    TotalLoanAmount = g.Sum(l => l.LoanAmount)
                })
                .ToListAsync();

            return Ok(result);

        }

        // LoanController.cs (API kısmı)
        [HttpGet("GetLoanStatsByMonth")]
        public async Task<ActionResult<IEnumerable<LoanStatByMonthModel>>> GetLoanStatsByMonth()
        {
            var result = await context.Loans
                .GroupBy(l => new { l.StartDate.Year, l.StartDate.Month })
                .Select(g => new LoanStatByMonthModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    LoanCount = g.Count(),
                    TotalLoanAmount = g.Sum(l => l.LoanAmount)
                })
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Month)
                .ToListAsync();

            return Ok(result);
        }

    }
}
