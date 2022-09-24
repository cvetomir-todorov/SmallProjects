using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Project.Testing.Actions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SqsImport : Attribute, ITestAction
{
    private readonly string _sqs;
    private readonly string _serializedFile;

    public SqsImport(string sqs, string serializedFile)
    {
        _sqs = sqs;
        _serializedFile = serializedFile;
    }

    public void BeforeTest(ITest test)
    {
        TestContext.WriteLine("Importing messages from '{0}' to SQS '{1}'", _serializedFile, _sqs);
    }

    public void AfterTest(ITest test) { }

    public ActionTargets Targets { get; } = ActionTargets.Test;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SqsDrain : Attribute, ITestAction
{
    private readonly string _sqs;

    public SqsDrain(string sqs)
    {
        _sqs = sqs;
    }
    
    public void BeforeTest(ITest test) { }

    public void AfterTest(ITest test)
    {
        TestContext.WriteLine("Draining SQS '{0}'", _sqs);
    }

    public ActionTargets Targets { get; } = ActionTargets.Test;
}
