# Task description

There is a Data API that returns the number of pages for a specific query. The Data API also returns the items for each page. Write code that returns all pages in parallel.

# Technology

* C# and .NET are used as a language and a platform
* Multi-threading programming tools used: 
  - `System.Threading.Task` and the Task Parallel Library (TPL)
  - `async-await` keywords for asynchronous programming
  - `System.Threading.Channels.Channel` as a means for exchanging data between different `Task` instances
    - It is used as an alternative to the concurrent collections and the `BlockingCollection`
  - `IAsyncEnumerable` in order to combine both `async-await` benefits and `IEnumerable` with `yield` keyword
  - `CancellationToken` to support cancellation triggered by the caller

# Implementation

* Data API contract:
  - supports returning the number of pages for the query as an async operation
  - supports returning a specific page for the query as an async operation
  - the items that are returned are simple DTOs
* Data API is implemented via a fake
  - returns a predetermined list of objects
  - simulates delay which is random
  - simulates a fake error at random
* The code returning all pages:
  - initializes an unbounded `Channel` with each page data representing an item in it
  - starts a long-running `Task` asynchronously
    - it in turn starts in parallel a `Task` for getting each page via the Data API async operation
    - each page is written to the unbounded `Channel`
    - after all parallel `Task` instances complete the `Channel` is marked as completed
  - begins reading the `Channel` for pages
  - returns asynchronously each item from each read page by utilizing the `IAsyncEnumerable` return type and `yield` keyword
  - supports cancellation via a `CancellationToken` passed to it
