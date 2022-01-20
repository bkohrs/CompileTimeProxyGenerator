using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CompileTimeProxyGenerator;

[Generator]
public class ProxySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("ProxyAttribute.cs", SourceText.From(@"
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

".Trim(), Encoding.UTF8));
        });
        var typeDeclarationSyntaxes = context.SyntaxProvider.CreateSyntaxProvider<TypeDeclarationSyntax?>(
            static (syntaxNode, _) =>
            {
                if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
                {
                    return typeDeclarationSyntax.AttributeLists.Count > 0;
                }

                return false;
            },
            static (generatorSyntaxContext, _) =>
            {
                if (generatorSyntaxContext.Node is TypeDeclarationSyntax typeDeclarationSyntax)
                {
                    foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
                    {
                        foreach (var attributeSyntax in attributeListSyntax.Attributes)
                        {
                            var attributeTypeSymbol = generatorSyntaxContext.SemanticModel
                                .GetSymbolInfo(attributeSyntax).Symbol?.ContainingType;
                            var attributeDisplayName = attributeTypeSymbol?.ToDisplayString();
                            if (attributeDisplayName == "CompileTimeProxyGenerator.ProxyAttribute")
                            {
                                return typeDeclarationSyntax;
                            }
                        }
                    }
                }

                return null;
            }).Where(node => node is not null);
        var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarationSyntaxes.Collect());
        context.RegisterSourceOutput(compilationAndTypes, (sourceProductionContext, tuple) =>
            Generate(sourceProductionContext, tuple.Item1, tuple.Item2));
    }

    private void Generate(SourceProductionContext sourceProductionContext, Compilation compilation,
        ImmutableArray<TypeDeclarationSyntax?> typeDeclarationSyntaxes)
    {
        if (typeDeclarationSyntaxes.IsDefaultOrEmpty)
            return;
        var distinctTypes = typeDeclarationSyntaxes
            .Where(typeSyntax => typeSyntax is not null)
            .Cast<TypeDeclarationSyntax>()
            .Distinct()
            .ToImmutableArray();
        var proxyInterfaceSymbol = compilation.GetTypeByMetadataName("CompileTimeProxyGenerator.ProxyAttribute");
        var voidTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var voidValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
        var typeTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        var typeValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");
        var assemblyName = Assembly.GetExecutingAssembly().GetName();

        foreach (var type in distinctTypes)
        {
            var model = compilation.GetSemanticModel(type.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(type) as INamedTypeSymbol;
            if (symbol == null)
                break;
            var attr = symbol.GetAttributes().FirstOrDefault(r =>
                SymbolEqualityComparer.Default.Equals(r.AttributeClass, proxyInterfaceSymbol));
            if (attr != null && attr.ConstructorArguments.Length == 2 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol interfaceArg)
            {
                var proxyAccessor = attr.ConstructorArguments[1].Value as string;
                var source = new StringBuilder();
                source.AppendLine($"namespace {symbol.ContainingNamespace.ToDisplayString()};");
                source.AppendLine();
                source.AppendLine(
                    $"[System.CodeDom.Compiler.GeneratedCode(\"{assemblyName.Name}\",\"{assemblyName.Version}\")]");
                source.AppendLine(
                    $"public partial class {symbol.Name} : {interfaceArg.ToDisplayString()}");
                source.AppendLine("{");
                var existingMembers = symbol.GetMembers();
                var interfaces = ImmutableArray.Create(interfaceArg).AddRange(interfaceArg.AllInterfaces);
                var members = interfaces.SelectMany(intf => intf.GetMembers()).ToImmutableArray();
                var existingProperties = existingMembers.OfType<IPropertySymbol>().ToImmutableArray();
                foreach (var property in members.OfType<IPropertySymbol>())
                {
                    if (existingProperties.Any(existing => PropertiesEqual(existing, property)))
                        continue;
                    source.AppendLine($"    public {property.Type.ToDisplayString()} {property.Name}");
                    source.AppendLine("    {");
                    if (property.GetMethod != null)
                        source.AppendLine($"        get => {proxyAccessor}.{property.Name};");
                    if (property.SetMethod != null)
                        source.AppendLine($"        set => {proxyAccessor}.{property.Name} = value;");
                    source.AppendLine("    }");
                }

                var existingMethods = existingMembers.OfType<IMethodSymbol>().ToImmutableArray();
                foreach (var member in members.OfType<IMethodSymbol>()
                             .Where(method => method.MethodKind == MethodKind.Ordinary))
                {
                    if (existingMethods.Any(existing => MethodsEqual(existing, member)))
                        continue;
                    var isAsync = SymbolEqualityComparer.Default.Equals(member.ReturnType, voidTaskSymbol) ||
                                  SymbolEqualityComparer.Default.Equals(member.ReturnType, voidValueTaskSymbol) ||
                                  (member.ReturnType is INamedTypeSymbol returnType &&
                                   (SymbolEqualityComparer.Default.Equals(returnType.ConstructedFrom, typeTaskSymbol) ||
                                    SymbolEqualityComparer.Default.Equals(returnType.ConstructedFrom,
                                        typeValueTaskSymbol)));
                    var parameters = string.Join(", ",
                        member.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}"));
                    var arguments = string.Join(", ", member.Parameters.Select(parameter => parameter.Name));
                    source.AppendLine(
                        $"    public {(isAsync ? "async " : "")}{member.ReturnType.ToDisplayString()} {member.Name}({parameters}) =>");
                    source.AppendLine(
                        $"        {(isAsync ? "await " : "")}{proxyAccessor}.{member.Name}({arguments}){(isAsync ? ".ConfigureAwait(false)" : "")};");
                }

                source.AppendLine("}");
                sourceProductionContext.AddSource(symbol.Name + ".cs", source.ToString());
            }
        }
    }

    private bool PropertiesEqual(IPropertySymbol property1, IPropertySymbol property2)
    {
        if (property1.Name != property2.Name)
            return false;
        if (!SymbolEqualityComparer.Default.Equals(property1.Type, property2.Type))
            return false;
        return true;
    }

    private static bool MethodsEqual(IMethodSymbol method1, IMethodSymbol method2)
    {
        if (method1.Name != method2.Name)
            return false;
        if (!SymbolEqualityComparer.Default.Equals(method1.ReturnType, method2.ReturnType))
            return false;
        if (method1.Parameters.Length != method2.Parameters.Length)
            return false;
        for (var i = 0; i < method1.Parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(method1.Parameters[i].Type, method2.Parameters[i].Type))
                return false;
        }

        return true;
    }
}