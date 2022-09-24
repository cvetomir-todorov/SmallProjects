namespace Project.Testing;

public static class Address
{
    public const string SqlServer = "test-sql-server";
    public const string DynamoDb = "test-dynamodb";
    public const string Sqs = "test-sqs";
}

public static class TestData
{
    public static class Csv
    {
        public const string File1 = "/path/to/file1.csv";
        public const string File2 = "/path/to/file2.csv";
    }

    public static class DataFile
    {
        public const string Data1 = "/path/to/data1.dat";
        public const string Data2 = "/path/to/data2.dat";
    }

    public static class Serialized
    {
        public const string Messages1 = "/path/to/serialized1.data";
        public const string Messages2 = "/path/to/serialized2.data";
    }

    public static class Kubernetes
    {
        public const string Env1 = "/path/to/k8s-env1.yml";
        public const string Env2 = "/path/to/k8s-env2.yml";
    }
}
