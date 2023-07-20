using System.Net;
using Common.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using HandGame.Api.Endpoints;
using HandGame.Api.Random;
using HandGame.Testing.Api.Infra;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RestSharp;

namespace HandGame.Testing.Api.Tests;

public class Choices : Base
{
    [Test]
    public async Task GetAll()
    {
        ChoicesResponse expectedResponse = new()
        {
            Choices = new[]
            {
                new ChoiceDto { Id = 0, Name = "rock" },
                new ChoiceDto { Id = 1, Name = "paper" },
                new ChoiceDto { Id = 2, Name = "scissors" },
                new ChoiceDto { Id = 3, Name = "lizard" },
                new ChoiceDto { Id = 4, Name = "spock" }
            }
        };

        RestRequest request = new("/choices");
        RestResponse<ChoicesResponse> response = await Client.ExecuteAsync<ChoicesResponse>(request);

        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [TestCase(0, 0, "rock")]
    [TestCase(1, 1, "paper")]
    [TestCase(2, 2, "scissors")]
    [TestCase(3, 3, "lizard")]
    [TestCase(4, 4, "spock")]
    public async Task Random(int generatedRandomValue, int expectedChoiceId, string expectedChoiceName)
    {
        StubRandom stubRandom = (StubRandom)App.ServiceProvider.GetRequiredService<IRandom>();
        stubRandom.Value = generatedRandomValue;

        RandomChoiceResponse expectedResponse = new()
        {
            Choice = new()
            {
                Id = expectedChoiceId,
                Name = expectedChoiceName
            }
        };

        RestRequest request = new("/choice");
        RestResponse<RandomChoiceResponse> response = await Client.ExecuteAsync<RandomChoiceResponse>(request);

        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
