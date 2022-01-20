//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public int MyProp2
    {
        get => _inner.MyProp2;
        set => _inner.MyProp2 = value;
    }
}
