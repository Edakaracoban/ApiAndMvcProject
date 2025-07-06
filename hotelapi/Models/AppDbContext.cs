using System;
using Microsoft.EntityFrameworkCore;

namespace hotelapi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanAmount)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LoginModel>().HasNoKey();
        }


        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Transaction> Transactions { get; set; }


        public DbSet<User> Users { get; set; }


    }
}