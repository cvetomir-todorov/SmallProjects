using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Project.Testing.Actions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DynamoDb : Attribute, ITestAction
{
    private readonly string _connString;
    private readonly string[] _dataFiles;

    public DynamoDb(string connString, params string[] dataFiles)
    {
        _connString = connString;
        _dataFiles = dataFiles;
    }

    public void BeforeTest(ITest test)
    {
        foreach (string dataFile in _dataFiles)
        {
            TestContext.WriteLine("Importing data from '{0}' into DynamoDB '{1}'", dataFile, _connString);
        }
    }

    public void AfterTest(ITest test)
    {
        TestContext.WriteLine("Cleaning up DynamoDB '{0}'", _connString);
    }

    public ActionTargets Targets { get; }
}
