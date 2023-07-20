namespace HandGame.Api.Random;

public interface IRandom
{
    /// <summary>
    /// Generates an instance of <see cref="System.Int32"/> with a random value.
    /// </summary>
    /// <exception cref="GenerateFailureException">An external random generator may not be able to respond (properly).</exception>
    Task<int> GenerateInt32(int min, int max);
}

public class GenerateFailureException : Exception
{
    private const string DefaultMessage = "Failed to generate a random value.";

    public GenerateFailureException() : base(DefaultMessage) { }
    public GenerateFailureException(string message) : base(message) { }
    public GenerateFailureException(Exception inner) : base(DefaultMessage, inner) { }
}
