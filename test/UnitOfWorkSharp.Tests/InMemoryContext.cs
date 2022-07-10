namespace UnitOfWorkSharp.Tests
{
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public class InMemoryContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid()
                .ToString(), new InMemoryDatabaseRoot());

    }
}