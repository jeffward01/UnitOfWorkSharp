namespace UnitOfWorkSharp.Tests
{
    using Entities;
    using Microsoft.EntityFrameworkCore;

    public class InMemoryContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseInMemoryDatabase("test");
    }
}