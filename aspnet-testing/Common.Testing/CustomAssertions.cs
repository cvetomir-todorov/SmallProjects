using System.Net;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using RestSharp;

namespace Common.Testing;

public static class CustomAssertions
{
    public static RestResponseAssertions<T> Should<T>(this RestResponse<T> instance)
    {
        return new RestResponseAssertions<T>(instance);
    }
}

public class RestResponseAssertions<T> : ReferenceTypeAssertions<RestResponse<T>, RestResponseAssertions<T>>
{
    public RestResponseAssertions(RestResponse<T> subject) : base(subject)
    { }

    protected override string Identifier => "REST response";

    public AndConstraint<RestResponseAssertions<T>> HaveStatusCode(
        HttpStatusCode expectedStatusCode,
        string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .Given(() => Subject.StatusCode)
            .ForCondition(actualStatusCode => actualStatusCode == expectedStatusCode)
            .FailWith("Expected {context:response} HTTP status code to be {0}{reason}, but found {1}. HTTP body is:{2}{3}",
                expectedStatusCode,
                Subject.StatusCode,
                Environment.NewLine,
                Subject.Content);

        return new AndConstraint<RestResponseAssertions<T>>(this);
    }
}
