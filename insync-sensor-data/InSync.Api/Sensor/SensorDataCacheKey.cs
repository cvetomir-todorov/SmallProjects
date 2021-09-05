using System;

namespace InSync.Api.Sensor
{
    internal readonly struct SensorDataCacheKey : IEquatable<SensorDataCacheKey>
    {
        private readonly byte _deviceID;
        private readonly DateTime _when;

        public SensorDataCacheKey(byte deviceID, DateTime when)
        {
            _deviceID = deviceID;
            _when = when;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 131_071;
                hash = hash * 524_287 + _deviceID.GetHashCode();
                hash = hash * 524_287 + _when.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is SensorDataCacheKey))
                return false;

            return Equals((SensorDataCacheKey) obj);
        }

        public bool Equals(SensorDataCacheKey other)
        {
            if (!_deviceID.Equals(other._deviceID))
                return false;
            if (!_when.Equals(other._when))
                return false;

            return true;
        }
    }
}