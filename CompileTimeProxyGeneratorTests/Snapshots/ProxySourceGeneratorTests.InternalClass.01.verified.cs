//HintName: MyInterfaceProxy.cs
namespace Test;

internal partial class MyInterfaceProxy : Test.IMyInterface
{
    public void MyMethod() =>
        _inner.MyMethod();
}
