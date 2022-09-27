using FluentValidation;

namespace World.Service.Endpoints.Continents;

public sealed class AddContinentRequest
{
    public string Name { get; set; } = string.Empty;
    public int Popularity { get; set; }
}

public class AddContinentRequestValidator : AbstractValidator<AddContinentRequest>
{
    public AddContinentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Popularity).GreaterThan(0);
    }
}
