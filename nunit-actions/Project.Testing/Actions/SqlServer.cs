using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Project.Testing.Actions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SqlServerImport : Attribute, ITestAction
{
    private readonly string _connString;
    private readonly string _csv;

    public SqlServerImport(string connString, string csv)
    {
        _connString = connString;
        _csv = csv;
    }

    public void BeforeTest(ITest test)
    {
        TestContext.WriteLine("Importing CSV '{0}' into SQL Server '{1}'", _csv, _connString);
    }

    public void AfterTest(ITest test)
    {
        // a separate NUnit action for clean up
    }

    public ActionTargets Targets { get; }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class SqlServerCleanUp : Attribute, ITestAction
{
    private readonly string _connString;

    public SqlServerCleanUp(string connString)
    {
        _connString = connString;
    }

    public void BeforeTest(ITest test)
    {
        // nothing to do here
    }

    public void AfterTest(ITest test)
    {
        TestContext.WriteLine("Cleaning up SQL Server '{0}'", _connString);
    }

    public ActionTargets Targets { get; }
}
