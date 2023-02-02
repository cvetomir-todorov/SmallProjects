namespace Crypto.Testing.Infra.Mocks;

public class TestDateTime : IDateTime
{
    private DateTime _initial = DateTime.UtcNow;
    private Queue<DateTime>? _sequence;
    private int _counter;

    public void SetInitial(DateTime initial)
    {
        _initial = initial;
    }

    public void SetNowSequence(Queue<DateTime> sequence)
    {
        _sequence = sequence;
    }

    public DateTime UtcNow()
    {
        if (_sequence == null)
        {
            // we simulate that processing a trade takes 1ms
            DateTime result = _initial.AddMilliseconds(_counter);
            _counter++;
            return result;
        }
        else
        {
            if (_sequence.Count == 0)
            {
                throw new InvalidOperationException("Too short datetime sequence provided.");
            }

            return _sequence.Dequeue();
        }
    }
}
