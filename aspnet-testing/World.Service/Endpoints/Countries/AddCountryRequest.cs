using FluentValidation;

namespace World.Service.Endpoints.Countries;

public sealed class AddCountryRequest
{
    public string ContinentName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public int CountryPopulation { get; set; }
}

public sealed class AddCountryRequestValidator : AbstractValidator<AddCountryRequest>
{
    public AddCountryRequestValidator()
    {
        RuleFor(x => x.ContinentName).NotEmpty();
        RuleFor(x => x.CountryName).NotEmpty();
        RuleFor(x => x.CountryPopulation).GreaterThan(0);
    }
}
