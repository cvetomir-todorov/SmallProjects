using System.Reflection;
using NUnit.Framework;
using Project.Testing.Actions;

namespace Project.Testing;

// When NUnit actions are applied at a suite level and have `ActionTargets.Test` they are executed for every test case within the suite
// `BeforeTest` is executed before actions applied on the test level
// `AfterTest` is executed after actions applied on the test level
[SqsImport(Address.Sqs, TestData.Serialized.Messages1)]
[SqsImport(Address.Sqs, TestData.Serialized.Messages2)]
[SqsDrain(Address.Sqs)]
public class EveryTest
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
