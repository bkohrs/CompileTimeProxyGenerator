//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public string MyProp
    {
        get => _inner.MyProp;
        set => _inner.MyProp = value;
    }
    public string MyBaseProp
    {
        get => _inner.MyBaseProp;
        set => _inner.MyBaseProp = value;
    }
    public void Dispose() =>
        _inner.Dispose();
}
