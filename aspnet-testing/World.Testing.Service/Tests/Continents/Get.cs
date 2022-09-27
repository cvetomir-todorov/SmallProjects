using System.Net;
using Common.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using RestSharp;
using World.Service.Endpoints.Continents;

namespace World.Testing.Service.Tests.Continents;

public class GetContinents : BaseTest
{
    protected override void AddTestData() 
    {
        DbContext.Continents.AddRange(
            new() {Id = 1, Name = "Europe", Popularity = 95},
            new() {Id = 2, Name = "Asia", Popularity = 80},
            new() {Id = 3, Name = "North America", Popularity = 70},
            new() {Id = 4, Name = "South America", Popularity = 85});

        DbContext.SaveChanges();
    }

    [Test]
    public async Task GetAllContinents()
    {
        // given
        ContinentsResponse expected = new()
        {
            Continents = new[]
            {
                new ContinentDto { Id = 1, Name = "Europe", Popularity = 95 },
                new ContinentDto { Id = 2, Name = "Asia", Popularity = 80 },
                new ContinentDto { Id = 3, Name = "North America", Popularity = 70 },
                new ContinentDto { Id = 4, Name = "South America", Popularity = 85 }
            }
        };

        // when
        RestRequest request = new("/api/continents/all");
        RestResponse<ContinentsResponse> response = await RestClient.ExecuteAsync<ContinentsResponse>(request);

        // then
        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expected);
        }
    }

    [Test]
    public async Task GetTopContinents()
    {
        ContinentsResponse expected = new()
        {
            Continents = new[]
            {
                new ContinentDto { Id = 1, Name = "Europe", Popularity = 95 },
                new ContinentDto { Id = 4, Name = "South America", Popularity = 85 }
            }
        };

        // when
        RestRequest request = new("/api/continents/top");
        RestResponse<ContinentsResponse> response = await RestClient.ExecuteAsync<ContinentsResponse>(request);

        // then
        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expected);
        }
    }
}
