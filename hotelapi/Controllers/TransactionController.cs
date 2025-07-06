using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotelapi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotelapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext context;

        public TransactionController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/transaction/GetTransactions?transactionType=Deposit
        [HttpGet]
        [Route("GetTransactions")]
        public async Task<IEnumerable<Transaction>> GetTransactions([FromQuery] string? transactionType)
        {
            var query = context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType.Contains(transactionType));
            }

            return await query.ToListAsync();
        }


        // GET: api/transaction/GetTransactionById/5
        [HttpGet]
        [Route("GetTransactionById/{id}")]
        public async Task<Transaction> GetTransactionById(int id)   
        {
            return await context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        // POST: api/transaction/AddTransaction
        [HttpPost]
        [Route("AddTransaction")]
        public async Task<Transaction> AddTransaction([FromBody] Transaction transaction)
        {
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
            return transaction;
        }

        // PUT: api/transaction/UpdateTransaction/5
        [HttpPut]
        [Route("UpdateTransaction/{id}")]
        public async Task<Transaction> UpdateTransaction(int id, [FromBody] Transaction transaction)
        {
            if (id != transaction.TransactionId)
                return null;

            context.Transactions.Update(transaction);
            await context.SaveChangesAsync();
            return transaction;
        }



        // DELETE: api/transaction/DeleteTransaction/5
        [HttpDelete]
        [Route("DeleteTransaction/{id}")]
        public async Task<bool> DeleteTransaction(int id)
        {
            var transaction = await context.Transactions.FindAsync(id);
            if (transaction == null)
                return false;

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            return true;
        }

        // Arama: TransactionType, Tarih aralığı, Amount aralığı
        // GET: api/transaction/Search?type=Deposit&min=100&start=2024-01-01&end=2024-12-31
        [HttpGet]
        [Route("Search")]
        public async Task<IEnumerable<Transaction>> Search(
            [FromQuery] string type,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end)
        {
            var query = context.Transactions
                .Include(t => t.Account)
                .ThenInclude(a => a.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.TransactionType.Contains(type));

            if (min.HasValue)
                query = query.Where(t => t.Amount >= min.Value);

            if (max.HasValue)
                query = query.Where(t => t.Amount <= max.Value);

            if (start.HasValue)
                query = query.Where(t => t.Date >= start.Value);

            if (end.HasValue)
                query = query.Where(t => t.Date <= end.Value);

            return await query.ToListAsync();
        }

        // Group By: TransactionType'e göre toplam tutar ve adet
        // GET: api/transaction/GetTransactionStatsByType
        [HttpGet]
        [Route("GetTransactionStatsByType")]
        public async Task<IEnumerable<object>> GetTransactionStatsByType()
        {
            var result = await context.Transactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    TransactionType = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .ToListAsync();

            return result;
        }
    }
}