using System.Reflection;
using NUnit.Framework;
using Project.Testing.Actions;

namespace Project.Testing;

// When NUnit actions are applied at a suite level and have ActionTargets.Suite they are executed exactly once for the whole suite
// `BeforeTest` is executed exactly once before all actions applied on the test level
// `AfterTest` is executed exactly once after all actions applied on the test level
[UseKubernetes(TestData.Kubernetes.Env1)]
public class OncePerSuite
{
    [Test]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File1)]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File2)]
    [SqlServerCleanUp(Address.SqlServer)]
    public void SqlServerOnly()
    {
        TestContext.WriteLine("Executing {0}", MethodBase.GetCurrentMethod()?.Name);
    }

    [Test]
    [DynamoDb(Address.DynamoDb, TestData.DataFile.Data1, TestData.DataFile.Data2)]
    public void DynamoDbOnly()
    {
        TestContext.WriteLine("Executing {0}", MethodBase.GetCurrentMethod()?.Name);
    }
}
