//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyMethod(object param1) =>
        _inner.MyMethod(param1);
}
