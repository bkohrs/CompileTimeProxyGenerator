//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public void MyMethod() =>
        _inner.MyMethod();
}
