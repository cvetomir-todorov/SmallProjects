using Microsoft.Extensions.Options;

namespace HandGame.Api.Random;

public class ExternalRandom : IRandom
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RandomOptions _options;

    public ExternalRandom(IHttpClientFactory httpClientFactory, IOptions<RandomOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<int> GenerateInt32(int min, int max)
    {
        if (min >= max)
        {
            throw new ArgumentException($"Min {min} should be strictly < max {max}.");
        }

        string url = string.Format(_options.Address, min, max);
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        using HttpClient client = _httpClientFactory.CreateClient("external-random");

        try
        {
            using HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                int[]? success = await response.Content.ReadFromJsonAsync<int[]>();
                if (success == null)
                {
                    throw new GenerateFailureException("Unexpected format of the response.");
                }
                else if (success.Length != 1)
                {
                    throw new GenerateFailureException($"Unexpected number count {success.Length} instead of just 1.");
                }
                else if (success[0] < min || success[0] > max)
                {
                    throw new GenerateFailureException($"Unexpected number {success[0]} instead of a value within [{min}, {max}].");
                }
                else
                {
                    return success[0];
                }
            }
            else
            {
                string body = await response.Content.ReadAsStringAsync();
                string exMsg = $"External random generator returned an unexpected status code {response.StatusCode} with body '{body}'";
                throw new GenerateFailureException(exMsg);
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            throw new GenerateFailureException(httpRequestException);
        }
    }
}
