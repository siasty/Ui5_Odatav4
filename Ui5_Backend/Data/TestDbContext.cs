using Microsoft.EntityFrameworkCore;
using Ui5_Backend.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ui5_Backend.Data
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
        {

        }
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("Test");
            }
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<ProductRating> Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         

            base.OnModelCreating(modelBuilder);
        }
    }
}
