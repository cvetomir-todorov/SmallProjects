# Slot machine game

Implemented by Cvetomir Todorov - [mail](mailto:cvetomir.todorov@gmail.com), [LinkedIn](https://www.linkedin.com/in/cvetomirtodorov/), [GitHub](https://github.com/cvetomir-todorov/)

# Task description

A simplified slot machine game has to be implemented. 

### Rules

* At the start of the game the player should enter the deposit amount (e.g. the initial money balance).
* After that, for each spin, the player is asked how much money he wants to stake.
* A table with the results of each spin is displayed to the player.
* The win amount should be displayed together with the total balance at the current stage.
* After each spin the total balance will be equal to: ({deposit amount} - {stake amount}) + {win amount}).
* Game ends when the player balance hits 0.

### The game

* A slot game with dimensions 4 rows of 3 symbols each.
* The symbols are placed randomly respecting the probability of each item.
* The player will win only if one or more horizontal lines contain 3 matching symbols. Wildcard (*) is a symbol that matches any other symbol (A, B or P).
* The won amount should be the sum of the coefficients of the symbols on the winning line(s), multiplied by the stake amount.
* Supports following symbols:

| Symbol        	| Coefficient 	| Probability 	|
|---------------	|------------:	|------------:	|
| Apple (A)     	|         0.4 	|         45% 	|
| Banana (B)    	|         0.6 	|         35% 	|
| Pineapple (P) 	|         0.8 	|         15% 	|
| Wildcard (*)  	|         0.0 	|          5% 	|

### Example game

* Please deposit money you would like to play with: 200
* Enter stake amount: 10
* Spinning and coefficients calculation
  - B A A = 0.0
  - A A A = 0.4 + 0.4 + 0.4 = 1.2
  - A * B = 0.0
  - \* A A = 0.0 + 0.4 + 0.4 = 0.8
* Player has staked 10 and winning coefficient is 1.2 + 0.8 = 2 so win is: 10 * 2 = 20.
* The won amount is then added to the current balance of the player: 190 + 20 = 210.

# Implementation

### Technologies

* .NET Core is preferred over .NET Framework for its Linux support, performance benefits, being relevant etc.
* Default .NET Core DI container does the job. If something more complicated is required AutoFac or another contianer could be integrated.
* CommandLineParser is used for parsing the command line and providing feedback when it is wrong.
* Configuration is stored in JSON using .NET functionality for obtaining it.
* Validation for the configuration is accomplished using the excellent FluentValidation library.
* Serilog is used for logging, although NLog could do the job, too. Serilog could be configured to use an ApplicationInsights sink in order to centralize logs. Additionally, log entries are enriched with machine ID.
* NUnit is preferred for testing over xUnit. xUnit may have very nice features but poor documentation tips the scales in favor of NUnit which also has thoughtful project maintainers and an excellent community.
* FluentAssertions library eases the assertions.

### Configuration

Game configuration is extracted into a `game.json` file with optional environment-based overwriting as normal. It contains:

* General section with:
  - Max amount - in [100 000, 10 000 000].
  - Amound precision - in [0, 4].
  - Coefficient precision - in [0, 4].
  - Currency - just a string.
* Spin section with:
  - Row count - in [1,8].
  - Symbol count - in [2,8].
* Symbol section with a list of symbols each of which has:
  - Type - normal/wildcard. There should be at least one wildcard.
  - Name - just a string.
  - Letter - single char, must be unique.
  - Coefficient - represented as an int. Logical range is [0,10]. If its precision is 2 then: 0.01 = 1, 1 = 100, 10 = 1000.
  - Probability - integer, total sum should be exactly 100.

Coefficients, row count, symbol count should allow theoretical winning. For more details take a look into configuration validation logic.

### Design

* Program and Startup follow the standard .NET Core practice. Startup is additionally designed to be used for testing.
* Configuration classes are simple DTOs with FluentValidation validators for them. Configuration is validated at start up.
* UI is implemented via separate `IOutput` and `IInteraction` interfaces.
  - Inputted amounts are always rounded as configured.
* Application flow:
  - Is designed as a state machine using the state design pattern
  - Employs the Flyweight design pattern as a central container for `State` instances.
* Spinning:
  - Uses a default `System.Random`-based implementation.
  - Employs the interpreter design pattern for symbol evaluation relying on a spin context for each symbol row.
  - Employs the Flyweight design pattern to store `Symbol` representations for the interpreter.
* Amount of money is stored in an `Amount` class:
  - It is immutable.
  - Keeps the amount value in `long` data type using the specified precision.
  - Each amount value is calculated as follows: Amount = Round(DecimalValue, Precision) * 10^Precision
  - Ensures amount is always within [0, MAX] range.
  - Avoids floating point inaccuracy in addition and subtraction operations.
  - For multiplication and division operations uses `decimal` data type and then converts back the value to `long` with flooring and rounding.
* The amounts representing the balance and stake are contained within a `Wallet` class which ensures validity and atomicity of operations.

### Testing

* Money testing includes micro tests for both `Amount` and `Wallet` classes.
* Spinning testing:
  - Overrides the default `Startup` with a record-based `IRandomNumberGenerator` and `IOutput`.
  - Resolves the `Spin` instance and uses its hierarchy registered in the DI container.
* Application flow testing:
  - Overrides the default `Startup` with a record-based `IRandomNumberGenerator` and `IOutput`.
  - Overrides the `GameEngine` with a custom one which captures application state and wallet balance and stakes after each state transition.
  - Overrides the `IInteraction` with a record-based one in order to simulate user input.

# Progress

### Source

* [x] Application setup - entry point, DI container, exception-handling, logging
* [x] Configuration including validation
* [x] UI implementation via console
* [x] Application flow via state transitions
* [x] Spinning and random number generation
* [x] Money handling - amount and wallet
* [x] Logging throughout the application
* [x] Comments

### Testing

* [x] Testing setup
* [x] Input validation 
* [x] Money
* [x] Spinning
* [x] Application flow
