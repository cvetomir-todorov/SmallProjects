using System.Reflection;
using NUnit.Framework;
using Project.Testing.Actions;

namespace Project.Testing;

public class SingleTest
{
    // These NUnit actions work with SQL Server for this test only
    // The NUnit import action imports data from a single file but can be applied multiple times
    // The NUnit cleanup action cleans the whole SQL Server and can be applied just once
    [Test]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File1)]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File2)]
    [SqlServerCleanUp(Address.SqlServer)]
    public void SqlServerOnly()
    {
        TestContext.WriteLine("Executing {0}", MethodBase.GetCurrentMethod()?.Name);
    }

    // This NUnit action works with DynamoDB for this test only
    // The NUnit action imports data from multiple files and can be applied just once
    // The NUnit action also cleans up the whole DynamoDB as well
    [Test]
    [DynamoDb(Address.DynamoDb, TestData.DataFile.Data1, TestData.DataFile.Data2)]
    public void DynamoDbOnly()
    {
        TestContext.WriteLine("Executing {0}", MethodBase.GetCurrentMethod()?.Name);
    }

    // Various NUnit actions for a single test can be applied
    // What is guaranteed is that:
    // * Each `BeforeTest` will be executed before test is ran
    // * Each `AfterTest` will be executed after test is ran 
    // Order of each `BeforeTest` and `AfterTest` within the respective set of methods is not guaranteed and depends on the .NET runtime
    [Test]
    [DynamoDb(Address.DynamoDb, TestData.DataFile.Data1, TestData.DataFile.Data2)]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File1)]
    [SqlServerImport(Address.SqlServer, TestData.Csv.File2)]
    [SqlServerCleanUp(Address.SqlServer)]
    public void Various()
    {
        TestContext.WriteLine("Executing {0}", MethodBase.GetCurrentMethod()?.Name);
    }
}
