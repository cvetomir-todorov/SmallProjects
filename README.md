# Small projects

Contains some interesting things I've done over the years.

# Applications

### Crypto trade engine using Binance trade feed

Trade engine processing crypto currency trades in real-time using Binance, detects volume spikes and passes the data downstream to RabbitMQ queues

Tags: C#, .NET, crypto, trading, volume spikes, real-time, performance, data-intensive, RabbitMQ, actor model, configuration files, NUnit, automated testing, unit testing, TDD, data-driven testing

### Hand game

Rock-paper-scissors-lizard-spock hand game using an external random generator

Tags: C#, .NET, hand game, RESTful API, observability, Open Telemetry, NUnit, automated testing, unit testing, TDD, data-driven testing

### InSync sensor data system

Distributed system for sending, receiving, processing, storing and searching sensor data

Tags: C#, .NET, multi-threading, synchronization, TCP, named-pipes, RESTful API, file system, binary-formatted storage, configuration files

### Slot machine game

Simplified slot machine game

Tags: C#, .NET, money amount handling, design patterns, console UI, automated testing, unit testing, TDD, data-driven testing, configuration files, validation, logging

# Automated testing

### Automated testing for ASP.NET applications

How automated testing for ASP.NET applications should be done via the API only, so that changing the internal implementation doesn't break the tests, which leads to improved productivity

Tags: C#, .NET, ASP.NET, NUnit, automated testing, unit testing, TDD, data-driven testing

### NUnit actions

Usage of NUnit actions in order to setup and tear down test data needed for data-driven tests

Tags: C#, .NET, NUnit, automated testing, unit testing, TDD, data-driven testing

# Experiments and measurements

### Linked-list implemented with an array

Implementation of the linked-list data structure via an array, which leads to an improved performance because of the fixed number of initial allocations, reduced GC pressure, cache locality

Tags: C#, .NET, data structures, linked-list

### Parallel data fetching

Example how to fetch pages of data in parallel

Tags: C#, .NET, Task Parallel Library, asynchronous programming, IAsyncEnumerable, threading channels, cancellation support

### Reflection performance benchmarking

Alternative ways of using reflection in a way that doesn't decrease performance when compared to regular static functions, class/interface methods, delegates

Tags: C#, .NET, reflection, benchmarking
