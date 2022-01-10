﻿using CompileTimeProxyGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace CompileTimeProxyGeneratorTests;

[TestFixture]
public class ProxySourceGeneratorTests
{
    [Test]
    public async Task Method_NoParameters_VoidReturn()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    void MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_NoParameters_WithReturnType()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_WithSingleParameter_WithReturnType()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyMethod(object param1);
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_WithMultipleParameter_WithReturnType()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyMethod(object param1, int param2, string param3);
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_ReturnsAsyncVoidTask()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    System.Threading.Tasks.Task MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_ReturnsAsyncTypedTask()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    System.Threading.Tasks.Task<string> MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_ReturnsAsyncVoidValueTask()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    System.Threading.Tasks.ValueTask MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Method_ReturnsAsyncTypedValueTask()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    System.Threading.Tasks.ValueTask<string> MyMethod();
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Property_ReadOnly()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyProp { get; }
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Property_WriteOnly()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyProp { set; }
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    [Test]
    public async Task Property_ReadWrite()
    {
        await RunGenerator(@"
namespace Test;

public interface IMyInterface
{
    string MyProp { get; set; }
}

[CompileTimeProxyGenerator.Proxy(typeof(IMyInterface), ""_inner"")]
public class MyInterfaceProxy : IMyInterface
{
    private IMyInterface _inner;
    public MyInterfaceProxy(IMyInterface inner)
    {
        _inner = inner;
    }
}
").ConfigureAwait(false);
    }

    private async Task RunGenerator(string code)
    {
        var referenceAssemblies = await ReferenceAssemblies.Default
            .ResolveAsync(LanguageNames.CSharp, CancellationToken.None).ConfigureAwait(false);

        var compilation =
            CSharpCompilation.Create("name", new[] { CSharpSyntaxTree.ParseText(code) }, referenceAssemblies);
        var generator = new ProxySourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        await Verify(driver)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("System.CodeDom.Compiler.GeneratedCode")
            .ConfigureAwait(false);
    }
}