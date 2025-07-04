using System;
using Microsoft.EntityFrameworkCore;

namespace hotelmvc.Models
{
	public class MvcDbContext:DbContext
	{
        public MvcDbContext(DbContextOptions<MvcDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginModel>().HasNoKey();

     
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<UserModel> UserModels { get; set; }
    }
}

