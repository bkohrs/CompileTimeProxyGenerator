//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyMethod() =>
        _inner.MyMethod();
}
