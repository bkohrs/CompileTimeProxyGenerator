//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public async System.Threading.Tasks.ValueTask MyMethod() =>
        await _inner.MyMethod().ConfigureAwait(false);
}
