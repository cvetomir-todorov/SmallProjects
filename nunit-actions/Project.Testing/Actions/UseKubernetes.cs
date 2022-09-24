using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Project.Testing.Actions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UseKubernetes : Attribute, ITestAction
{
    private readonly string _yaml;

    public UseKubernetes(string yaml)
    {
        _yaml = yaml;
    }

    public void BeforeTest(ITest test)
    {
        TestContext.WriteLine("Starting Kubernetes using '{0}'", _yaml);
    }

    public void AfterTest(ITest test)
    {
        TestContext.WriteLine("Stopping Kubernetes started using '{0}'", _yaml);
    }

    public ActionTargets Targets { get; } = ActionTargets.Suite;
}
