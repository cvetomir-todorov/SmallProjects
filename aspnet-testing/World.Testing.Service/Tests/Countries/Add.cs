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

public class AddCountry : BaseTest
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
    public async Task AddValidCountry()
    {
        // given
        CountryDto expected = new()
        {
            Id = 4, Name = "Malta", Population = 500_000,
            Continent = new ContinentDto { Id = 1, Name = "Europe", Popularity = 95 }
        };
        AddCountryRequest addCountryRequest = new()
        {
            CountryName = expected.Name,
            CountryPopulation = expected.Population,
            ContinentName = expected.Continent.Name
        };

        // when adding country
        RestRequest postRequest = new("api/countries", Method.Post);
        postRequest.AddJsonBody(addCountryRequest);
        RestResponse<CountryDto> postResponse = await RestClient.ExecuteAsync<CountryDto>(postRequest);

        // then returned country
        using (new AssertionScope())
        {
            postResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            postResponse.Data.Should().BeEquivalentTo(expected);
        }

        // when getting added country
        RestRequest getRequest = new($"api/countries/{expected.Id}");
        RestResponse<CountryDto> getResponse = await RestClient.ExecuteAsync<CountryDto>(getRequest);

        // then added country
        using (new AssertionScope())
        {
            getResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            getResponse.Data.Should().BeEquivalentTo(expected);
        }
    }

    [TestCase("invalid country name", null, 500_000, "Europe", HttpStatusCode.BadRequest)]
    [TestCase("invalid country population", "Malta", 0, "Europe", HttpStatusCode.BadRequest)]
    [TestCase("invalid continent name", "Malta", 500_000, null, HttpStatusCode.BadRequest)]
    [TestCase("non existing continent", "Malta", 500_000, "Non existing", HttpStatusCode.Conflict)]
    public async Task AddInvalidCountry(string testName, string countryName, int countryPopulation, string continentName, HttpStatusCode expectedStatusCode)
    {
        // given
        AddCountryRequest addCountryRequest = new()
        {
            CountryName = countryName,
            CountryPopulation = countryPopulation,
            ContinentName = continentName
        };

        RestRequest getRequest = new("api/countries");
        RestResponse<CountriesResponse> getResponse = await RestClient.ExecuteAsync<CountriesResponse>(getRequest);
        using (new AssertionScope())
        {
            getResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            getResponse.Data?.Countries.Length.Should().Be(3);
        }
        CountryDto[] existingCountries = getResponse.Data!.Countries;

        // when attempting to add country
        RestRequest postRequest = new("api/countries", Method.Post);
        postRequest.AddJsonBody(addCountryRequest);
        RestResponse<CountryDto> response = await RestClient.ExecuteAsync<CountryDto>(postRequest);

        // then attempt should fail
        response.Should().HaveStatusCode(expectedStatusCode);

        // when getting existing countries
        getResponse = await RestClient.ExecuteAsync<CountriesResponse>(getRequest);

        // then existing should be the same count
        using (new AssertionScope())
        {
            getResponse.Should().HaveStatusCode(HttpStatusCode.OK);
            getResponse.Data?.Countries.Length.Should().Be(existingCountries.Length);
        }
    }
}
