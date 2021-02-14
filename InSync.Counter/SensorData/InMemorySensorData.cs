using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;

namespace InSync.Counter.SensorData
{
    /// <summary>
    /// Sensor data storage before it is being processed.
    /// </summary>
    public interface ISensorData
    {
        void AddData(byte deviceID, byte sensorData);

        IDictionary<byte, ICollection<byte>> GetData();
    }

    public sealed class InMemorySensorData : ISensorData
    {
        private readonly int _sensorDataCapacity;
        private ConcurrentDictionary<byte, ICollection<byte>> _backend;

        public InMemorySensorData(IOptions<SensorDataOptions> options)
        {
            // calculate collection capacity for a hint and avoid resizing
            _sensorDataCapacity = (byte) Math.Ceiling(options.Value.PersistInterval / options.Value.MinSendInterval + 1);
            _backend = new ConcurrentDictionary<byte, ICollection<byte>>();
        }

        public void AddData(byte deviceID, byte sensorData)
        {
            ICollection<byte> targetValues = _backend.GetOrAdd(deviceID, key => new SensorValues(_sensorDataCapacity));
            targetValues.Add(sensorData);
        }

        public IDictionary<byte, ICollection<byte>> GetData()
        {
            IDictionary<byte, ICollection<byte>> currentBackend = _backend;
            Interlocked.Exchange(ref _backend, new ConcurrentDictionary<byte, ICollection<byte>>());
            return currentBackend;
        }
    }

    public sealed class SensorValues : ICollection<byte>
    {
        private readonly List<byte> _values;
        private readonly object _valuesLock;

        public SensorValues(int capacity)
        {
            _values = new List<byte>(capacity);
            _valuesLock = new object();
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(byte item)
        {
            lock (_valuesLock)
            {
                _values.Add(item);
            }
        }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public void Clear() => throw new NotSupportedException();
        public bool Contains(byte item) => throw new NotSupportedException();
        public void CopyTo(byte[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(byte item) => throw new NotSupportedException();
    }
}
