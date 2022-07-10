namespace UnitOfWorkSharp.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Collections;
    using Entities;
    using Xunit;

    public class IQueryablePageListExtensionsTests
    {
        public List<Customer> TestItems() =>
            new List<Customer>
            {
                new Customer
                {
                    Name = "A",
                    Age = 1
                },
                new Customer
                {
                    Name = "B",
                    Age = 1
                },
                new Customer
                {
                    Name = "C",
                    Age = 2
                },
                new Customer
                {
                    Name = "D",
                    Age = 3
                },
                new Customer
                {
                    Name = "E",
                    Age = 4
                },
                new Customer
                {
                    Name = "F",
                    Age = 5
                }
            };

        [Fact]
        public async Task ToPagedListAsyncTest()
        {
            await using var db = new InMemoryContext();
            var testItems = TestItems();
            await db.AddRangeAsync(testItems);
            await db.SaveChangesAsync();

            var items = db.Customers.Where(t => t.Age > 1);

            var page = await items.ToPagedListAsync(1, 2);
            Assert.NotNull(page);

            Assert.Equal(4, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal("E", page.Items[0]
                .Name);

            page = await items.ToPagedListAsync(0, 2);
            Assert.NotNull(page);
            Assert.Equal(4, page.TotalCount);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal("C", page.Items[0]
                .Name);
            await db.Database.EnsureDeletedAsync();
        }
    }
}