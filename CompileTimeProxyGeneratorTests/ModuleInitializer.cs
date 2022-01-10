using System.Runtime.CompilerServices;

namespace CompileTimeProxyGeneratorTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();
    }
}
//
// namespace CompileTimeProxyGenerator
// {
//     [System.AttributeUsage(AttributeTargets.Class)]
//     internal class ProxyAttribute : System.Attribute
//     {
//         public ProxyAttribute(Type proxyType, string proxyAccessor)
//         {
//             ProxyType = proxyType;
//             ProxyAccessor = proxyAccessor;
//         }
//
//         public Type ProxyType { get; }
//         public string ProxyAccessor { get; }
//     }
// }
// public interface IMyInterface
// {
//     void MyMethod();
// }
//
// [CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), "_inner")]
// public class MyInterfaceProxy : IMyInterface
// {
//     private IMyInterface _inner;
//     public MyInterfaceProxy(IMyInterface inner)
//     {
//         _inner = inner;
//     }
// }