//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyProp
    {
        get => _inner.MyProp;
        set => _inner.MyProp = value;
    }
}
