using System.Net;
using Common.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using RestSharp;
using World.Service.Database.Continents;
using World.Service.Endpoints.Continents;
using World.Service.Endpoints.Countries;

namespace World.Testing.Service.Tests.Countries;

public class GetCountries : BaseTest
{
    protected override void AddTestData()
    {
        ContinentEntity europe = new() { Id = 1, Name = "Europe", Popularity = 95 };
        ContinentEntity southAmerica = new() { Id = 4, Name = "South America", Popularity = 85 };

        DbContext.Continents.AddRange(europe, southAmerica);
        DbContext.Countries.AddRange(
            new() { Id = 1, Name = "Bulgaria", Population = 6_000_000, Continent = europe },
            new() { Id = 2, Name = "Romania", Population = 19_000_000, Continent = europe },
            new() { Id = 3, Name = "Brazil", Population = 216_000_000, Continent = southAmerica });

        DbContext.SaveChanges();
    }

    [Test]
    public async Task GetAllCountries()
    {
        // given
        ContinentDto europe = new() {Id = 1, Name = "Europe", Popularity = 95};
        ContinentDto southAmerica = new() {Id = 4, Name = "South America", Popularity = 85};

        CountriesResponse expected = new()
        {
            Countries = new[]
            {
                new CountryDto { Id = 1, Name = "Bulgaria", Population = 6_000_000, Continent = europe },
                new CountryDto { Id = 2, Name = "Romania", Population = 19_000_000, Continent = europe },
                new CountryDto { Id = 3, Name = "Brazil", Population = 216_000_000, Continent = southAmerica }
            }
        };

        // when
        RestRequest request = new("/api/countries");
        RestResponse<CountriesResponse> response = await RestClient.ExecuteAsync<CountriesResponse>(request);

        // then
        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expected);
        }
    }

    public static object[] GetSingleCountryTestCases =
    {
        new object?[]
        {
            "existing", 3, HttpStatusCode.OK, new CountryDto
            {
                Id = 3, Name = "Brazil", Population = 216_000_000,
                Continent = new ContinentDto { Id = 4, Name = "South America", Popularity = 85 }
            }
        },
        new object?[]
        {
            "non-existing", 123456789, HttpStatusCode.NotFound, null
        }
    };

    [TestCaseSource(nameof(GetSingleCountryTestCases))]
    public async Task GetSingleCountry(string testName, int countryId, HttpStatusCode expectedStatusCode, CountryDto expectedCountry)
    {
        // given

        // when
        RestRequest request = new($"/api/countries/{countryId}");
        RestResponse<CountryDto> response = await RestClient.ExecuteAsync<CountryDto>(request);

        // then
        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(expectedStatusCode);
            if (expectedStatusCode == HttpStatusCode.OK)
            {
                response.Data.Should().BeEquivalentTo(expectedCountry);
            }
        }
    }
}
