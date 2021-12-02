# InSync sensor data system

### Task description

Create an embedded system for sending, receiving, processing, storing and searching sensor data.

### Functional requirements

* Multiple instances of device sensor
  - Each sensor has a device ID
  - Each sensor send its data at a predefined interval of time
* A single instance counter server
  - It gathers data from all sensors
  - Processes sensor data by calculating average for each sensor for an interval
  - Persists sensor data and average value for each sensor for an interval
  - Persists data on a predefined interval of time
  - Outputs in the console the connected sensors and their data for each interval
  - May be started and stopped
* A single instance API
  - Search sensor data for a device ID and for a single point of time
  - Start and stop the counter server

### Non-functional requirements

* Implement the system using .NET Core and C#
* Each component is configurable via a file
* Sensors communicate with counter via TCP
* Sensors
  - Sensors would not be more than 256
  - Sensors generate random sensor data
  - Interval at which sensors send data to the counter would be between 1 to 5 seconds
  - All sensors use the same interval
  - The full sensor data packet is 3 bytes: 0xAA for synchronization, 1 byte device ID, 1 byte sensor data
* Counter
  - Stores the data into an in-memory data structure
  - Should persist sensor data for all sensors in a file containing the date and time
  - The choice for the format of the file is free
  - The interval at which counter persist sensor data would be between 5 and 120 seconds
  - The sensor average for an interval could be rounded to a byte rather than a float/double
  - Missed sensor data while the counter is stopped is not used and doesn't need to be retransmitted
* API
  - Should be RESTful

# Implementation

### Technologies

* Default .NET Core DI container is sufficient for this task.
* CommandLineParser is used for parsing the command line and providing feedback when it is wrong.
* Configuration is stored in JSON using .NET functionality for obtaining it.
* NLog is used for logging to the console. There was no requirement to have file logs and a new NLog file target can achieve that, if needed.
* Swashbuckle is used to generate OpenAPI (Swagger) info for the API.

### Configuration

#### Sensor

* Sender
  - SendDataInterval - how often the sensor data is sent
  - SendDataTimeout - used to configure the TCP client send timeout
  - ReconnectInterval - how often auto-reconnect is tried

#### Counter

* Control
  - PipeName - the name of the named pipe used for communication between Counter and API
* SensorData
  - BufferPoolSize - the size of the buffer pool used by the TCP server
  - SensorIdleTimeout - the interval after which sensor is considered dead if no data has been received from it
  - MinSendInterval - should be 1s, the minimum interval for the sensor to send data
  - PersistInterval - how often sensor data is persisted
  - PersistDirectory - where sensor data is persisted

#### API

* Counter
  - PipeName - the name of the named pipe used for communication between Counter and API
  - PipeReconnectInterval - how often reconnect is tried
  - PipeConnectTimeout - the timeout for connecting to the named pipe
* Sensor
  - PersistInterval - how often sensor data is persisted
  - DataDirectory - where sensor data is read from

### Design

#### Common

* Program and Startup follow the standard .NET Core practice with the addition that their names are also prefixed.
* Sensor and Counter have command line arguments classes.
* Throughout the components various multi-threading approaches for synchronization are used, such as: `Interlocked`, `SemaphoreSlim`, `lock`, `CancellationTokenSource`, `Timer`, `ConcurrentDictionary` etc.
* File names for persisted sensor data have normalized date time which is described in detail in `SensorDataUtil`.
* Format of the sensor data is binary and is specified in detail in the implementation `ReadMe.txt`.

#### Sensor

* The sensor simply starts up two `Timer` instances
  - One periodically tries to send the sensor data generated via `Random` instance.
  - Another periodically tries to auto-reconnect if the `TcpClient` isn't already created and connected.

#### Counter

* Exposes a simplified command-line interface allowing it to be stopped, started again or exited.
* Creates a named pipe and listens for control commands from the API which allow the counter to be stopped and started.
* Starts a TCP listener:
  - It accepts sensor connection initializations.
  - Using the async-await paradigm creates an event-based server.
  - Each sensor data send is queued on the thread-pool and processed via its IO threads.
* The sensor data is kept in an in-memory data structure as required
  - It is represented as a concurrent dictionary
  - Key = sensor device ID
  - Value = synchronized list with sensor values
* Each interval sensor data is atomically switched with a new one
* Sensor data is processed after the interval is over
* Sensor data and processed derived data are persisted in a file

#### API

* Exposes OpenAPI endpoints using Swashbuckle and Swagger via `/swagger`
* Exposes an `/api/counter-status` resource supporting `GET` and `PUT` for getting and updating counter status respectively.
* The counter client automatically tries to reconnect in a defined interval.
* Exposes an `/api/sensor/{deviceID}/data` resource supporting `GET` with `when={datetime}` query string
  - It caches the sensor data for the pair (deviceID, when)

# Progress

### Sensor

* [x] Command-line
* [x] Configuration
* [x] Logging
* [x] Startup and main
* [x] TCP communication
* [x] Send data each interval
* [x] Stop
* [x] Auto-reconnect

### Counter

* [x] Command-line
* [x] Configuration
* [x] Logging
* [x] Startup and main
* [x] TCP communication
* [x] Accept sensors
* [x] Process each sensor
* [x] Log when sensor connects/disconnects
* [x] Remove idle sensors
* [x] Store sensor data in memory
* [x] Calculate sensor data average
* [x] Persist sensor data each interval
* [x] Log sensor data each interval
* [x] Support start/stop of sensor data receiver
* [x] Listen to start/stop requests

### API

* [x] Configuration
* [x] Logging
* [x] Swagger
* [x] Startup and main
* [x] Find sensor data
* [x] Cache sensor data
* [x] Start/stop counter

### Base project

* [x] Persist sensor data in binary format in file
* [x] Data file name handling
* [x] Named pipe messaging
