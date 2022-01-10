//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyProp
    {
        set => _inner.MyProp = value;
    }
}
