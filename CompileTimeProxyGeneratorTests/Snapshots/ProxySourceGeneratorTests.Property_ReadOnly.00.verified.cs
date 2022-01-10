//HintName: ProxyAttribute.cs

namespace CompileTimeProxyGenerator;

[System.AttributeUsage(AttributeTargets.Class)]
internal class ProxyAttribute : System.Attribute
{
    public ProxyAttribute(System.Type proxyType, string proxyAccessor)
    {
        ProxyType = proxyType;
        ProxyAccessor = proxyAccessor;
    }

    public System.Type ProxyType { get; }
    public string ProxyAccessor { get; }
}

