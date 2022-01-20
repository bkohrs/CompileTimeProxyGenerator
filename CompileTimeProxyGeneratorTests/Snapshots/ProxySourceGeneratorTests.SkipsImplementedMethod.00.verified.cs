//HintName: ProxyAttribute.cs
using System;

namespace CompileTimeProxyGenerator;

[AttributeUsage(AttributeTargets.Class)]
internal class ProxyAttribute : Attribute
{
    public ProxyAttribute(Type proxyType, string proxyAccessor)
    {
        ProxyType = proxyType;
        ProxyAccessor = proxyAccessor;
    }

    public Type ProxyType { get; }
    public string ProxyAccessor { get; }
}