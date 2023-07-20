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

public class Playing : Base
{
    [TestCase(0, 0, "tie")]
    [TestCase(0, 1, "lose")]
    [TestCase(0, 2, "win")]
    [TestCase(0, 3, "win")]
    [TestCase(0, 4, "lose")]
    [TestCase(1, 0, "win")]
    [TestCase(1, 1, "tie")]
    [TestCase(1, 2, "lose")]
    [TestCase(1, 3, "lose")]
    [TestCase(1, 4, "win")]
    [TestCase(2, 0, "lose")]
    [TestCase(2, 1, "win")]
    [TestCase(2, 2, "tie")]
    [TestCase(2, 3, "win")]
    [TestCase(2, 4, "lose")]
    [TestCase(3, 0, "lose")]
    [TestCase(3, 1, "win")]
    [TestCase(3, 2, "lose")]
    [TestCase(3, 3, "tie")]
    [TestCase(3, 4, "win")]
    [TestCase(4, 0, "win")]
    [TestCase(4, 1, "lose")]
    [TestCase(4, 2, "win")]
    [TestCase(4, 3, "lose")]
    [TestCase(4, 4, "tie")]
    public async Task Valid(int playerChoice, int botChoice, string expectedResult)
    {
        StubRandom stubRandom = (StubRandom)App.ServiceProvider.GetRequiredService<IRandom>();
        stubRandom.Value = botChoice;

        PlayResponse expectedResponse = new()
        {
            Player = playerChoice,
            Bot = botChoice,
            Result = expectedResult
        };

        RestRequest request = new("/play", Method.Post);
        request.AddJsonBody(new PlayRequest { Player = playerChoice });
        RestResponse<PlayResponse> response = await Client.ExecuteAsync<PlayResponse>(request);

        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            response.Data.Should().BeEquivalentTo(expectedResponse);
        }
    }

    [TestCase(5)]
    [TestCase(255)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public async Task Invalid(int playerChoice)
    {
        RestRequest request = new("/play", Method.Post);
        request.AddJsonBody(new PlayRequest { Player = playerChoice });
        RestResponse<PlayResponse> response = await Client.ExecuteAsync<PlayResponse>(request);

        using (new AssertionScope())
        {
            response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
        }
    }
}
