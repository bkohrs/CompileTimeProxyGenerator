//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyMethod(object param1, int param2, string param3) =>
        _inner.MyMethod(param1, param2, param3);
}
