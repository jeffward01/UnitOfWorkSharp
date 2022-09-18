namespace UnitOfWorkSharp.Tests;

using Collections;
using Entities;
using Xunit;

public class IQueryablePageListExtensionsTests
{
    public List<Customer> TestItems() =>
        new()
        {
            new()
            {
                Name = "A",
                Age = 1
            },
            new()
            {
                Name = "B",
                Age = 1
            },
            new()
            {
                Name = "C",
                Age = 2
            },
            new()
            {
                Name = "D",
                Age = 3
            },
            new()
            {
                Name = "E",
                Age = 4
            },
            new()
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
        Assert.Equal(
            "E",
            page.Items[0]
                .Name);

        page = await items.ToPagedListAsync(0, 2);
        Assert.NotNull(page);
        Assert.Equal(4, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(
            "C",
            page.Items[0]
                .Name);
    }
}