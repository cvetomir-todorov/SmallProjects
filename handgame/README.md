# Description

Implements the popular [Rock-paper-scissors-lizard-spock game](https://www.wikihow.com/Play-Rock-Paper-Scissors-Lizard-Spock). Its rules could also [be watched being explained by Sheldon Cooper in The Big Bang Theory TV series](https://www.youtube.com/watch?v=pIpmITBocfM). Below are the endpoints of the game:

### `GET /choices`

Returns all possible choices

```json
{
  "choices": [
    { "id": 0, "name": "rock" },
    { "id": 1, "name": "paper" },
    { "id": 2, "name": "scissors" },
    { "id": 3, "name": "lizard" },
    { "id": 4, "name": "spock" }
  ]
}
```

### `GET /choice`

Returns a randomly generated choice

```json
{
  "choice": { "id": 0, "name": "rock" }
}
```

### `POST /play`

Plays against the computer

Request body

```json
{
  "player": 0
}
```

Response body

```json
{
  "player": 0,
  "bot": 0,
  "result": "win/lose/tie"
}
```

# Design

* Endpoints module with separate DTOs, flyweight design pattern for the choices, output caching, [FluentValidation](https://github.com/FluentValidation/FluentValidation)-based input validation
* Game module with its own domain objects, interface segregation
* Random module with config options and 2 implementations
* Telemetry module for the custom metrics
* Generic and reusable code in projects starting with `Common` in their name
* https://www.randomnumberapi.com is used for generating a random number
* The HTTP client is created via a custom `HttpClientFactory` with some [Polly](https://github.com/App-vNext/Polly) policies for some resilience

# Automated testing

There are data-driven unit tests. The unit is the whole component - the API itself. Here are [the pros/cons of this approach](https://github.com/cvetomir-todorov/Workflow/blob/main/docs/engineering-automated-testing.md). Ian Cooper also has [a tech talk about this](https://www.youtube.com/watch?v=EZ05e7EMOLM). A [custom piece of code related to testing the whole ASP.NET application](https://github.com/cvetomir-todorov/SmallProjects/tree/main/aspnet-testing) is being used. [NUnit](https://github.com/nunit/nunit) is the test framework and the assertions are based on [FluentAssertions](https://github.com/fluentassertions/fluentassertions). The tests focus on correctness. There is a stub in order to control the randomness.

# Security

* The API doesn't support users
* There are [scripts to generate TLS certificate](source/certificates) which are needed before code can be built/ran and they work on Ubuntu, probably other distros or Mac but probably not on Windows
* In terms of rate limiting there are 2 calls allowed within 5s from the same IP for each game endpoint, which is probably too much if a lot of users are using the service since the provided external random generator employs aggressive rate limiting as well

# Observability

## Logging

Logging is achieved via [Serilog](https://github.com/serilog/serilog) but there are not a lot of custom log messages and certainly they've not been made allocation-free.

## Health

There is a detailed `/healthz` endpoint, but also `/livez` and `/readyz` ones too.

## Distributed tracing

[OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet) is used to add distributed tracing with ASP.NET and HTTP client instrumentation. There is sampling which is always-on during development and 50% in the docker deployment. Traces are exported to a Jaeger instance which is accessible from the browser at port `16686`.

## Metrics

[OpenTelemetry](https://github.com/open-telemetry/opentelemetry-dotnet) is used to support metrics for ASP.NET, HTTP client and custom ones which count the wins, ties and losses. They're accessible from the Prometheus instance which scrapes them (Grafana hasn't been added) and they can be seen in the `/metrics` endpoint on the service.

# Packaging

The API is containerized, but the image size isn't reduced apart from the fact a two-staged build is used. Script should be ran from the dir where it resides, otherwise the paths should get fixed first.

# Deployment

There is a docker compose file for dev which has 3 instances: 1 service, 1 jaeger, 1 prometheus.
