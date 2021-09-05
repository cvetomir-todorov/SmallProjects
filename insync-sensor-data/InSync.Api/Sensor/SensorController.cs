using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using InSync.Api.Infrastructure;
using InSync.SensorDataStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Api.Sensor
{
    public sealed class SensorDataResponse
    {
        public byte DeviceID { get; set; }

        public byte Average { get; set; }

        [JsonConverter(typeof(ByteArrayConverter))]
        public byte[] SensorValues { get; set; }
    }

    [ApiController]
    [Route("api/sensor")]
    public sealed class SensorController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ISensorDataUtil _sensorDataUtil;
        private readonly ISensorDataReader _dataReader;
        private readonly IMemoryCache _memoryCache;
        private readonly SensorOptions _options;

        public SensorController(
            ILogger<SensorController> logger,
            ISensorDataUtil sensorDataUtil,
            ISensorDataReader dataReader,
            IMemoryCache memoryCache,
            IOptions<SensorOptions> options)
        {
            _logger = logger;
            _sensorDataUtil = sensorDataUtil;
            _dataReader = dataReader;
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        [HttpGet("{deviceID}/data")]
        public async Task<ActionResult> GetSensorData([FromRoute] byte deviceID, [FromQuery] DateTime when)
        {
            // ASP.NET could convert date time parameters to local time, more here: https://stackoverflow.com/q/10293440/608971
            if (when.Kind == DateTimeKind.Local)
            {
                when = when.ToUniversalTime();
            }

            DateTime normalizedWhen = _sensorDataUtil.Normalize(when, _options.PersistInterval);
            SensorDataCacheKey key = new SensorDataCacheKey(deviceID, normalizedWhen);
            SensorDataResult searchResult;
            if (_memoryCache.TryGetValue(key, out SensorDataResult cachedResult))
            {
                _logger.LogDebug("Use cached data for sensor {0} for {1}.", deviceID, when);
                searchResult = cachedResult;
            }
            else
            {
                string fileName = _sensorDataUtil.GetDataFileName(when, _options.PersistInterval);
                string filePath = Path.Combine(_options.DataDirectory, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Data for sensor {deviceID} was not found for the specified date time {when}.");
                }

                searchResult = await _dataReader.FindSensorData(deviceID, filePath);
                _memoryCache.Set(key, searchResult);
            }

            ActionResult actionResult = ProcessSearchResult(deviceID, when, searchResult);
            return actionResult;
        }

        private ActionResult ProcessSearchResult(byte deviceID, DateTime when, SensorDataResult searchResult)
        {
            if (!searchResult.IsFound)
            {
                return NotFound($"Data for sensor {deviceID} was not found for the specified date time {when}.");
            }

            SensorDataResponse response = new SensorDataResponse
            {
                DeviceID = deviceID,
                Average = searchResult.Average,
                SensorValues = searchResult.SensorValues
            };

            return Ok(response);
        }
    }
}
