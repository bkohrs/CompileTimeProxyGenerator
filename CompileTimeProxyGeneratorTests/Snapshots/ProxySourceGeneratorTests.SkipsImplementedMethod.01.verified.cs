//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public int MyMethod2(int b) =>
        _inner.MyMethod2(b);
}
