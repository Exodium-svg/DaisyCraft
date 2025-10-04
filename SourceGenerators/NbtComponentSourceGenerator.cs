using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nbt.SourceGen
{
    [Generator]
    public class NbtComponentSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var candidates = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is TypeDeclarationSyntax tds && tds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                    transform: static (ctx, _) =>
                    {
                        var typeDecl = (TypeDeclarationSyntax)ctx.Node;
                        var symbol = ctx.SemanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                        return symbol;
                    })
                .Where(static symbol => symbol is not null && ImplementsINbtComponent(symbol))
                .Collect();

            context.RegisterSourceOutput(candidates, static (spc, symbols) =>
            {
                foreach (var symbol in symbols.Distinct())
                {
                    if (symbol is not null)
                    {
                        var source = GenerateNbtComponentImplementation(symbol);
                        spc.AddSource($"{symbol.Name}_NbtComponent.g.cs", source);
                    }
                }
            });
        }

        private static bool ImplementsINbtComponent(INamedTypeSymbol? symbol)
        {
            if (symbol == null) return false;
            return symbol.AllInterfaces.Any(i => i.Name == "INbtComponent");
        }

        private static string GenerateNbtComponentImplementation(INamedTypeSymbol typeSymbol)
        {
            var ns = typeSymbol.ContainingNamespace.ToDisplayString();
            var typeKind = typeSymbol.TypeKind == TypeKind.Struct ? "struct" : "class";
            var typeName = typeSymbol.Name;

            var properties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "NbtComponentTypeAttribute"))
                .ToList();

            var writeMethod = GenerateWriteMethod(properties);
            var readMethod = GenerateReadMethod(properties);

            return $@"
using Nbt.Tags;

namespace {ns}
{{
    partial {typeKind} {typeName}
    {{
        public void Write(NbtWriter writer)
        {{
{writeMethod}
        }}

        public void Read(ref readonly NbtCompound tag)
        {{
{readMethod}
        }}
    }}
}}
";
        }

        private static string GenerateWriteMethod(List<IPropertySymbol> properties)
        {
            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                var attr = property.GetAttributes().First(a => a.AttributeClass?.Name == "NbtComponentTypeAttribute");
                var key = attr.ConstructorArguments[0].Value?.ToString() ?? property.Name;
                var componentType = attr.ConstructorArguments[1].Value?.ToString();

                sb.AppendLine($"            if ({property.Name} != null)");
                sb.AppendLine($"                writer.WriteTag(new Nbt{componentType}(\"{key}\", {property.Name}));");
            }
            return sb.ToString();
        }

        private static string GenerateReadMethod(List<IPropertySymbol> properties)
        {
            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                var attr = property.GetAttributes().First(a => a.AttributeClass?.Name == "NbtComponentTypeAttribute");
                var key = attr.ConstructorArguments[0].Value?.ToString() ?? property.Name;
                var componentType = attr.ConstructorArguments[1].Value?.ToString();

                var localName = property.Name.ToLowerInvariant();
                sb.AppendLine($"            if (tag.TryGetValue(\"{key}\", out var {localName}Tag) && {localName}Tag is Nbt{componentType} {localName}Value)");
                sb.AppendLine($"                {property.Name} = {localName}Value.Value;");
            }
            return sb.ToString();
        }
    }
}