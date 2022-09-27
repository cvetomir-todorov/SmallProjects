# Description

* Example how automated testing for ASP.NET applications should be done
* An approach backed by [Ian Cooper](https://mvp.microsoft.com/en-us/PublicProfile/8975?fullName=Ian%20Cooper) multiple talks about [doing TDD the right way](https://www.youtube.com/watch?v=EZ05e7EMOLM)
* Benefits of this testing infrastructure include:
  - It tests the external API of a service, not the internal implementation
    - Refactoring the internals wouldn't break the tests
    - Only API or behavior change could break the tests
  - Both the test and production code can be debugged
  - It can be ran both locally and in the CICD pipeline

# Design

## `AspNetApplication` class

* The web application is bootstrapped and it reuses the `Startup` class
  - Alternatively, a `TestStartup` can be created by inheriting the one in the production code and modifying it
* The web application could be configured according to the specific test needs
  - The initial `IHostBuilder` can be changed
    - By default the implementation uses `Host.CreateDefaultBuilder`
  - The URL setting is required in order to start Kestrel
    - It is also obtainable via the `BaseUrl` property
  - Dependency injection container registrations could be amended or overwritten
  - Logging could be customized
  - Custom application configuration is also supported
* Test code can use the web application instance to create an `HttpClient`
  - It already has the base address set to the correct URL
  - Clients should only specify the path, e.g. `api/continents`
  - The HTTP `User-agent` header is also set
  - Clients could, if they wish, wrap the created `HttpClient` into a custom HTTP client such as [RestSharp](https://github.com/restsharp/RestSharp)
* This implementation is reusable in different projects and is stored in a separate `Common.Testing` project

## Lifetime for the `AspNetApplication`

* Because of its configurability each different instance of `AspNetApplication` may have its own, different from the others:
  - Initial `IHostBuilder`
  - URL where it is listening for requests
  - Dependency injection container registrations
  - Logging configuration
  - Custom application configuration
* Based on the needs the test infrastructure:
  - Could start many `AspNetApplication` instances for the same web application under test
  - They could have different configurations
  - They would suit to the different testing needs
* In terms of lifetime each `AspNetApplication` can be started and then disposed:
  - Once for all tests in the test assembly
  - Once for a whole test suite
  - Once for each test
  - ... or any other way possible through writing code that would make sense in the specific scenario
* In the example here:
  - A `Program` class at root level is used to setup and cleanup the testing environment before and after the whole run
  - A `BaseTest` class creates, starts and disposes the `AspNetApplication` which means lifetime is for the whole test suite which inherits this `BaseTest` class

## Integration and E2E testing

* The configurability of the simple `AspNetApplication` makes it easy to write integration and even E2E tests
* If a solution contains multiple ASP.NET applications each of them can be started for a given (set of) test(s)
* For example:
  - A BFF serving the front end
  - And 3 data services behind it
  - Could all be tested together
* Of course the challenge always is what to do with external dependencies such as:
  - Databases
  - Message brokers
  - File systems
  - External services
* The choices usually boil down to:
  - Set up a real but test database/message broker/file system/external service
    - Preferably it should be a new unique one for each **run** in order to strengthen test isolation
    - It can be cleaned up between different test cases
  - Create a client with an interface in the production code for the external dependency
    - Overwrite the dependency injection registration with a stub/mock for the tests
    - The stub/mock is more easily configured what data to return and how to behave
