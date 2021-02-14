using System;

namespace InSync.SensorDataStorage
{
    public interface ISensorDataUtil
    {
        /// <summary>
        /// Calculates a normalized fixed point of time when given a random point of time and an interval.
        /// For example if the interval is 5 seconds for each point of time the resulting normalized one
        /// would have seconds that are divisible by the 5 second interval.
        /// 2020-01-25 14:35:17 -> 2020-01-25 14:35:20
        /// 2020-01-25 14:35:19 -> 2020-01-25 14:35:20
        /// 2020-01-25 14:35:15 -> 2020-01-25 14:35:15
        /// 2020-01-25 14:35:20 -> 2020-01-25 14:35:20
        /// This is used in order to consistently use the correct sensor data file for a given interval of time.
        /// </summary>
        DateTime Normalize(DateTime when, TimeSpan persistInterval);

        string GetDataFileName(DateTime when, TimeSpan persistInterval);
    }

    public sealed class SensorDataUtil : ISensorDataUtil
    {
        public DateTime Normalize(DateTime when, TimeSpan interval)
        {
            long whenTicks = when.Ticks;
            long intervalTicks = interval.Ticks;

            long remainder = whenTicks % intervalTicks;
            if (remainder == 0)
            {
                return when;
            }
            else
            {
                long deltaTicks = intervalTicks - remainder;
                whenTicks += deltaTicks;
                return DateTime.FromBinary(whenTicks);
            }
        }

        public string GetDataFileName(DateTime when, TimeSpan persistInterval)
        {
            DateTime normalized = Normalize(when, persistInterval);
            string fileName = $"counts-{normalized:yyyy-MMdd-HHmmss}.ext";
            return fileName;
        }
    }
}
