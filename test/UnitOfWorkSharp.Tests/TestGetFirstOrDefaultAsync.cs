﻿namespace UnitOfWorkSharp.Tests;

using Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class TestGetFirstOrDefaultAsync
{
    protected static List<Country> TestCountries =>
        new()
        {
            new Country
            {
                Id = 1,
                Name = "A"
            },
            new Country
            {
                Id = 2,
                Name = "B"
            }
        };

    public static List<City> TestCities =>
        new()
        {
            new City
            {
                Id = 1,
                Name = "A",
                CountryId = 1
            },
            new City
            {
                Id = 2,
                Name = "B",
                CountryId = 2
            },
            new City
            {
                Id = 3,
                Name = "C",
                CountryId = 1
            },
            new City
            {
                Id = 4,
                Name = "D",
                CountryId = 2
            },
            new City
            {
                Id = 5,
                Name = "E",
                CountryId = 1
            },
            new City
            {
                Id = 6,
                Name = "F",
                CountryId = 2
            }
        };

    public static List<Town> TestTowns =>
        new()
        {
            new Town
            {
                Id = 1,
                Name = "TownA",
                CityId = 1
            },
            new Town
            {
                Id = 2,
                Name = "TownB",
                CityId = 2
            },
            new Town
            {
                Id = 3,
                Name = "TownC",
                CityId = 3
            },
            new Town
            {
                Id = 4,
                Name = "TownD",
                CityId = 4
            },
            new Town
            {
                Id = 5,
                Name = "TownE",
                CityId = 5
            },
            new Town
            {
                Id = 6,
                Name = "TownF",
                CityId = 6
            }
        };

    [Fact]
    public async void TestGetFirstOrDefaultAsyncCanInclude()
    {
        var db = await LoadTestDataAsync();
        var repository = new Repository<City>(db);
        var city = await repository.GetFirstOrDefaultAsync(c => c.Name == "A", include: source => source.Include(t => t.Towns));
        Assert.NotNull(city);
        Assert.NotNull(city.Towns);
    }


    [Fact]
    public async void TestGetFirstOrDefaultAsyncGetsCorrectItem()
    {
        var db = await LoadTestDataAsync();
        var repository = new Repository<City>(db);

        var city = await repository.GetFirstOrDefaultAsync(t => t.Name == "A");
        Assert.NotNull(city);
        Assert.Equal(1, city.Id);
    }

    [Fact]
    public async void TestGetFirstOrDefaultAsyncReturnsNullValue()
    {
        var db = await LoadTestDataAsync();
        var repository = new Repository<City>(db);
        var city = await repository.GetFirstOrDefaultAsync(t => t.Name == "Easy-E");
        Assert.Null(city);
    }

    private async Task<InMemoryContext> LoadTestDataAsync()
    {
        var db = new InMemoryContext();
        if (db.Countries.Any() == false)
        {
            db.AddRange(TestCountries);
            db.AddRange(TestCities);
            db.AddRange(TestTowns);
            await db.SaveChangesAsync();
        }

        return db;
    }
}