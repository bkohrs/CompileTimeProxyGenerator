//HintName: MyInterfaceProxy.cs
namespace Test;

public partial class MyInterfaceProxy : Test.IMyInterface
{
    public async System.Threading.Tasks.Task MyMethod() =>
        await _inner.MyMethod().ConfigureAwait(false);
}
