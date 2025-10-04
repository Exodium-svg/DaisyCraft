using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
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
            //var readMethod = GenerateReadMethod(properties);
            var readMethod = string.Empty;//GenerateReadMethod(properties);

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
                var enumConstant = attr.ConstructorArguments[1];
                var componentType = enumConstant.Kind == TypedConstantKind.Enum
                    ? enumConstant.Type?.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(enumConstant.Value) == true)?
                        .Name
                    : null;

                // Check if the property is nullable
                bool isNullable = property.Type.NullableAnnotation == NullableAnnotation.Annotated;

                switch (componentType)
                {
                    case "IntColor":
                        if (isNullable)
                        {
                            sb.AppendLine($"            if ({property.Name}.HasValue)");
                            sb.AppendLine($"                writer.WriteTag(new NbtInt(\"{key}\", ({property.Name}.Value.A << 24 | {property.Name}.Value.R << 16 | {property.Name}.Value.G << 8 | {property.Name}.Value.B)));");
                        }
                        else
                        {
                            sb.AppendLine($"            writer.WriteTag(new NbtInt(\"{key}\", ({property.Name}.A << 24 | {property.Name}.R << 16 | {property.Name}.G << 8 | {property.Name}.B)));");
                        }
                        break;

                    case "HexColor":
                        if (isNullable)
                        {
                            sb.AppendLine($"            if ({property.Name}.HasValue)");
                            sb.AppendLine($"                writer.WriteTag(new NbtString(\"{key}\", $\"#{{{property.Name}.Value.R:X2}}{{{property.Name}.Value.G:X2}}{{{property.Name}.Value.B:X2}}\"));");
                        }
                        else
                        {
                            sb.AppendLine($"            writer.WriteTag(new NbtString(\"{key}\", $\"#{{{property.Name}.R:X2}}{{{property.Name}.G:X2}}{{{property.Name}.B:X2}}\"));");
                        }
                        break;

                    case "Bool":
                        if (isNullable)
                        {
                            sb.AppendLine($"            if ({property.Name}.HasValue)");
                            sb.AppendLine($"                writer.WriteTag(new NbtByte(\"{key}\", (byte)({property.Name}.Value ? 1 : 0)));");
                        }
                        else
                        {
                            sb.AppendLine($"            writer.WriteTag(new NbtByte(\"{key}\", (byte)({property.Name} ? 1 : 0)));");
                        }
                        break;

                    case "String":
                        sb.AppendLine($"            if ({property.Name} != null)");
                        sb.AppendLine($"                writer.WriteTag(new NbtString(\"{key}\", {property.Name}));");
                        break;

                    default:
                        sb.AppendLine($"            // Unsupported type: {componentType}");
                        sb.AppendLine($"            throw new NotImplementedException(\"Serialization for {componentType} is not implemented.\");");
                        break;
                }
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

                switch (componentType)
                {
                    case "HexColor":
                    case "IntColor":
                        sb.AppendLine($"            if (tag.TryGetValue(\"{key}\", out var {localName}Tag) && {localName}Tag is NbtInt {localName}Value)");
                        sb.AppendLine($"                {property.Name} = System.Drawing.Color.FromArgb({localName}Value.Value);");
                        break;
                    case "Bool":
                        sb.AppendLine($"            if (tag.TryGetValue(\"{key}\", out var {localName}Tag) && {localName}Tag is NbtByte {localName}Value)");
                        sb.AppendLine($"                {property.Name} = {localName}Value.Value != 0;");
                        break;
                    case "String":
                        sb.AppendLine($"            if (tag.TryGetValue(\"{key}\", out var {localName}Tag) && {localName}Tag is NbtString {localName}Value)");
                        sb.AppendLine($"                {property.Name} = {localName}Value.Value;");
                        break;
                    default:
                        sb.AppendLine($"            // Unsupported type: {componentType}");
                        sb.AppendLine($"            throw new NotImplementedException(\"Deserialization for {componentType} is not implemented.\");");
                        break;
                }
            }
            return sb.ToString();
        }
    }
}