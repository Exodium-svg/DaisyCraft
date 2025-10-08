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
                        var source = GenerateNbtComponentImplementation(symbol, spc);
                        spc.AddSource($"{symbol.Name}_NbtComponent.g.cs", source);
                    }
                }
            });
        }

        private static void Log(SourceProductionContext context, string message)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "NBTGEN001",
                title: "Nbt SourceGen Log",
                messageFormat: "{0}",
                category: "NbtSourceGen",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, message));
        }

        private static bool ImplementsINbtComponent(INamedTypeSymbol? symbol)
        {
            if (symbol == null) return false;
            return symbol.AllInterfaces.Any(i => i.Name == "INbtComponent");
        }

        private static (bool isStruct, bool isNullableStruct, bool isReferenceType)
            AnalyzeNullability(IPropertySymbol property)
        {
            var type = property.Type;

            if (type.IsValueType)
            {
                // Nullable<T> is a special value type
                if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    return (isStruct: true, isNullableStruct: true, isReferenceType: false);
                return (isStruct: true, isNullableStruct: false, isReferenceType: false);
            }

            // Reference types (classes/interfaces)
            return (isStruct: false, isNullableStruct: false, isReferenceType: true);
        }

        private static string GenerateNbtComponentImplementation(INamedTypeSymbol typeSymbol, SourceProductionContext context)
        {
            var ns = typeSymbol.ContainingNamespace.ToDisplayString();
            var typeKind = typeSymbol.TypeKind == TypeKind.Struct ? "struct" : "class";
            var typeName = typeSymbol.Name;

            var properties = typeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "NbtComponentTypeAttribute"))
                .ToList();

            Log(context, $"Generating NbtComponent implementation for {typeName} with {properties.Count} properties.");

            var writeMethod = GenerateWriteMethod(properties);
            var readMethod = GenerateReadMethod(properties);
            var toCompoundMethod = GenerateToCompoundMethod(properties);

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

        public NbtCompound ToCompound(string? key = null)
        {{
            {toCompoundMethod}
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

                var (isStruct, isNullableStruct, isReferenceType) = AnalyzeNullability(property);
                string access = isNullableStruct ? ".Value" : "";
                bool needsCheck = isNullableStruct || isReferenceType;

                void EmitChecked(string codeBlock)
                {
                    if (needsCheck)
                    {
                        string check = isNullableStruct ? $"{property.Name}.HasValue" : $"{property.Name} != null";
                        sb.AppendLine($"            if ({check})");
                        sb.AppendLine($"            {{");
                        sb.AppendLine(codeBlock);
                        sb.AppendLine($"            }}");
                    }
                    else
                    {
                        sb.AppendLine(codeBlock);
                    }
                }

                switch (componentType)
                {
                    case "IntColor":
                        EmitChecked($"writer.WriteTag(new NbtInt(\"{key}\", ({property.Name}{access}.A << 24 | {property.Name}{access}.R << 16 | {property.Name}{access}.G << 8 | {property.Name}{access}.B)));");
                        break;

                    case "HexColor":
                        EmitChecked($"writer.WriteTag(new NbtString(\"{key}\", $\"#{{{property.Name}{access}.R:X2}}{{{property.Name}{access}.G:X2}}{{{property.Name}{access}.B:X2}}\"));");
                        break;

                    case "Bool":
                        EmitChecked($"writer.WriteTag(new NbtByte(\"{key}\", (byte)({property.Name}{access} ? 1 : 0)));");
                        break;

                    case "Byte":
                        EmitChecked($"writer.WriteTag(new NbtByte(\"{key}\", {property.Name}{access}));");
                        break;

                    case "Short":
                        EmitChecked($"writer.WriteTag(new NbtShort(\"{key}\", {property.Name}{access}));");
                        break;

                    case "Int":
                        EmitChecked($"writer.WriteTag(new NbtInt(\"{key}\", {property.Name}{access}));");
                        break;

                    case "Long":
                        EmitChecked($"writer.WriteTag(new NbtLong(\"{key}\", {property.Name}{access}));");
                        break;

                    case "Float":
                        EmitChecked($"writer.WriteTag(new NbtFloat(\"{key}\", {property.Name}{access}));");
                        break;

                    case "Double":
                        EmitChecked($"writer.WriteTag(new NbtDouble(\"{key}\", {property.Name}{access}));");
                        break;

                    case "String":
                        EmitChecked($"writer.WriteTag(new NbtString(\"{key}\", {property.Name}));");
                        break;

                    case "ByteArray":
                        EmitChecked($"writer.WriteTag(new NbtByteArray(\"{key}\", {property.Name}));");
                        break;

                    case "IntArray":
                        EmitChecked($"writer.WriteTag(new NbtIntArray(\"{key}\", {property.Name}));");
                        break;

                    case "LongArray":
                        EmitChecked($"writer.WriteTag(new NbtLongArray(\"{key}\", {property.Name}));");
                        break;

                    case "NbtArray":
                            var elementType = property.Type is IArrayTypeSymbol at ? at.ElementType : null;

                            bool isNbt = elementType != null && elementType.AllInterfaces.Any(i => i.Name == "INbtComponent");
                            bool isCompound = elementType?.Name == "NbtCompound";
                            bool isString = elementType?.SpecialType == SpecialType.System_String;

                            string code;
                            if (isNbt && !isCompound && !isString)
                            {
                                code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var compoundArray = new INbtTag[length];
        for (int i = 0; i < length; i++)
        {
            compoundArray[i] = {{property.Name}}[i].ToCompound(null);
        }
        writer.WriteTag(new NbtList("{{key}}", TagType.Compound, compoundArray));
        """;
                            }
                            else if (isCompound)
                            {
                                code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var compoundArray = new INbtTag[length];
        for (int i = 0; i < length; i++)
        {
            compoundArray[i] = {{property.Name}}[i];
        }
        writer.WriteTag(new NbtList("{{key}}", TagType.Compound, compoundArray));
        """;
                            }
                            else
                            {
                                // Generic primitives or strings
                                code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var array = new INbtTag[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = new NbtString("", {{property.Name}}[i].ToString());
        }
        writer.WriteTag(new NbtList("{{key}}", TagType.String, array));
        """;
                            }

                            EmitChecked(code);
                            break;
                    case "NbtComponent":
                        EmitChecked($"writer.WriteTag({property.Name}{access}.ToCompound(\"{key}\"));");
                        break;

                    default:
                        sb.AppendLine($"            // Unsupported type: {componentType}");
                        sb.AppendLine($"            throw new System.NotImplementedException(\"Serialization for {componentType} is not implemented.\");");
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
                var elementType = property.Type is IArrayTypeSymbol arrayType
                    ? arrayType.ElementType
                    : property.Type;
                var attr = property.GetAttributes().First(a => a.AttributeClass?.Name == "NbtComponentTypeAttribute");
                var key = attr.ConstructorArguments[0].Value?.ToString() ?? property.Name;
                var enumConstant = attr.ConstructorArguments[1];
                var componentType = enumConstant.Kind == TypedConstantKind.Enum
                    ? enumConstant.Type?.GetMembers()
                        .OfType<IFieldSymbol>()
                        .FirstOrDefault(f => f.HasConstantValue && f.ConstantValue?.Equals(enumConstant.Value) == true)?
                        .Name
                    : null;

                var (isStruct, isNullableStruct, isReferenceType) = AnalyzeNullability(property);
                var localName = property.Name.ToLowerInvariant();
                var valueName = $"{localName}Value";

                sb.AppendLine($"            if (tag.TryGetValue(\"{key}\", out var {localName}Tag))");
                sb.AppendLine($"            {{");

                switch (componentType)
                {
                    case "IntColor":
                        sb.AppendLine($"                if ({localName}Tag is NbtInt {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    int argb = {valueName}.Value;");
                        sb.AppendLine($"                    var color = System.Drawing.Color.FromArgb((argb >> 24) & 0xFF, (argb >> 16) & 0xFF, (argb >> 8) & 0xFF, argb & 0xFF);");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = color;");
                        else
                            sb.AppendLine($"                    {property.Name} = color;");
                        sb.AppendLine($"                }}");
                        break;

                    case "HexColor":
                        sb.AppendLine($"                if ({localName}Tag is NbtString {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    string hex = {valueName}.Value;");
                        sb.AppendLine($"                    if (hex.Length == 7 && hex[0] == '#' &&");
                        sb.AppendLine($"                        int.TryParse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null, out int r) &&");
                        sb.AppendLine($"                        int.TryParse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out int g) &&");
                        sb.AppendLine($"                        int.TryParse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, null, out int b))");
                        sb.AppendLine($"                    {{");
                        sb.AppendLine($"                        var color = System.Drawing.Color.FromArgb(r, g, b);");
                        if (isNullableStruct)
                            sb.AppendLine($"                        {property.Name} = color;");
                        else
                            sb.AppendLine($"                        {property.Name} = color;");
                        sb.AppendLine($"                    }}");
                        sb.AppendLine($"                }}");
                        break;

                    case "Bool":
                        sb.AppendLine($"                if ({localName}Tag is NbtByte {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value != 0;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value != 0;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Byte":
                        sb.AppendLine($"                if ({localName}Tag is NbtByte {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Short":
                        sb.AppendLine($"                if ({localName}Tag is NbtShort {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Int":
                        sb.AppendLine($"                if ({localName}Tag is NbtInt {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Long":
                        sb.AppendLine($"                if ({localName}Tag is NbtLong {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Float":
                        sb.AppendLine($"                if ({localName}Tag is NbtFloat {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "Double":
                        sb.AppendLine($"                if ({localName}Tag is NbtDouble {valueName})");
                        sb.AppendLine($"                {{");
                        if (isNullableStruct)
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        else
                            sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "String":
                        sb.AppendLine($"                if ({localName}Tag is NbtString {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "ByteArray":
                        sb.AppendLine($"                if ({localName}Tag is NbtByteArray {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "IntArray":
                        sb.AppendLine($"                if ({localName}Tag is NbtIntArray {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "LongArray":
                        sb.AppendLine($"                if ({localName}Tag is NbtLongArray {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    {property.Name} = {valueName}.Value;");
                        sb.AppendLine($"                }}");
                        break;

                    case "NbtArray":
                        //if (Debugger.IsAttached)
                        //    Debugger.Break();
                        //else
                        //    Debugger.Launch();

                        bool isNbt = property.ContainingType.AllInterfaces.Where((i) => i.Name == "INbtComponent").FirstOrDefault() != null;
                        bool isCompound = elementType.Name == "NbtCompound";
                        bool isString = elementType.SpecialType == SpecialType.System_String;

                        string readCode;

                        if(isNbt && !isCompound && !isString)
                        {
                            readCode = $$"""
                                var length = {{valueName}}Tag.Value.Length;
                                {{property.Name}} = new {{elementType}}[length];
                                for (int i = 0; i < length; i++)
                                {
                                    var condition = new {{elementType}}();
                                    NbtCompound compound = (NbtCompound){{valueName}}Tag.Value[i];
                                    condition.Read(ref compound);
                                    {{property.Name}}[i] = condition;
                                }
                            """;
                        }
                        else if(isCompound)
                        {
                            readCode = $$"""
                                var length = {{valueName}}Tag.Value.Length;
                                {{property.Name}} = new NbtCompound[length];
                                for (int i = 0; i < length; i++)
                                {
                                    NbtCompound compound = (NbtCompound){{valueName}}Tag.Value[i];
                                    {{property.Name}}[i] = compound;
                                }
                            """;
                        }
                        else
                        {
                            readCode = $$"""
                                var length = {{valueName}}Tag.Value.Length;
                                {{property.Name}} = new {{elementType}}[length];

                                for (int i = 0; i < length; i++) 
                                {
                                    {{property.Name}}[i] = (NbtString){{valueName}}Tag.Value[i];
                                }
                                """;
                        }
                        sb.AppendLine($$"""
                        if ({{localName}}Tag != null &&  {{localName}}Tag is NbtList {{valueName}}Tag)
                        {
                            {{readCode}}
                        }
""");
                        //sb.AppendLine($"                if ({localName}Tag is NbtList {valueName})");
                        //sb.AppendLine($"                {{");
                        //sb.AppendLine($"                    var length = {valueName}.Value.Length;");
                        //sb.AppendLine($"                    {property.Name} = new {elementType}[length];");
                        //sb.AppendLine($"                    for (int i = 0; i < length; i++) {{");
                        //sb.AppendLine($"                        var condition = new {elementType}();");
                        //sb.AppendLine($"                        NbtCompound compound = (NbtCompound){valueName}.Value[i];");
                        //sb.AppendLine($"                        condition.Read(ref compound);");
                        //sb.AppendLine($"                        {property.Name}[i] = condition;");
                        //sb.AppendLine($"                    }}");
                        //sb.AppendLine($"                }}");
                        break;

                    case "Compound":
                        sb.AppendLine($"                if ({localName}Tag is NbtCompound {valueName})");
                        sb.AppendLine($"                {{");
                        sb.AppendLine($"                    {property.Name} = {valueName};");
                        sb.AppendLine($"                }}");
                        break;

                    case "NbtComponent":
                        if (isNullableStruct)
                        {
                            // For nullable structs, we need to create a new instance and assign it
                            var underlyingType = ((INamedTypeSymbol)property.Type).TypeArguments[0];
                            sb.AppendLine($$"""
                            var temp{{property.Name}} = new {{underlyingType}}();
                            var localCompound{{property.Name}} = (NbtCompound){{localName}}Tag;
                            temp{{property.Name}}.Read(ref localCompound{{property.Name}});
                            {{property.Name}} = temp{{property.Name}};
                            """);
                        }
                        else
                        {
                            sb.AppendLine($$"""
                            {{property.Name}} = new {{property.Type}}();
                            var localCompound{{property.Name}} = (NbtCompound){{localName}}Tag;
                            {{property.Name}}.Read(ref localCompound{{property.Name}});
                            """);
                        }
                        break;

                    default:
                        sb.AppendLine($"                // Unsupported type: {componentType}");
                        sb.AppendLine($"                throw new System.NotImplementedException(\"Deserialization for {componentType} is not implemented.\");");
                        break;
                }
                sb.AppendLine($"            }}");

                // Only set to null if it's a nullable type (either nullable struct or reference type)
                if (isNullableStruct || isReferenceType)
                {
                    sb.AppendLine($"            else");
                    sb.AppendLine($"            {{");
                    sb.AppendLine($"                {property.Name} = default;");
                    sb.AppendLine($"            }}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GenerateToCompoundMethod(List<IPropertySymbol> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"            var compound = new NbtCompound(key ?? string.Empty);");

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

                var (isStruct, isNullableStruct, isReferenceType) = AnalyzeNullability(property);
                string access = isNullableStruct ? ".Value" : "";
                bool needsCheck = isNullableStruct || isReferenceType;

                void EmitChecked(string codeBlock)
                {
                    if (needsCheck)
                    {
                        string check = isNullableStruct ? $"{property.Name}.HasValue" : $"{property.Name} != null";
                        sb.AppendLine($"            if ({check})");
                        sb.AppendLine($"            {{");
                        sb.AppendLine(codeBlock);
                        sb.AppendLine($"            }}");
                    }
                    else
                    {
                        sb.AppendLine(codeBlock);
                    }
                }

                switch (componentType)
                {
                    case "IntColor":
                        EmitChecked($"compound[\"{key}\"] = new NbtInt(\"{key}\", ({property.Name}{access}.A << 24 | {property.Name}{access}.R << 16 | {property.Name}{access}.G << 8 | {property.Name}{access}.B));");
                        break;
                    case "HexColor":
                        EmitChecked($"compound[\"{key}\"] = new NbtString(\"{key}\", $\"#{{{property.Name}{access}.R:X2}}{{{property.Name}{access}.G:X2}}{{{property.Name}{access}.B:X2}}\");");
                        break;
                    case "Bool":
                        EmitChecked($"compound[\"{key}\"] = new NbtByte(\"{key}\", (byte)({property.Name}{access} ? 1 : 0));");
                        break;
                    case "Byte":
                        EmitChecked($"compound[\"{key}\"] = new NbtByte(\"{key}\", {property.Name}{access});");
                        break;
                    case "Short":
                        EmitChecked($"compound[\"{key}\"] = new NbtShort(\"{key}\", {property.Name}{access});");
                        break;
                    case "Int":
                        EmitChecked($"compound[\"{key}\"] = new NbtInt(\"{key}\", {property.Name}{access});");
                        break;
                    case "Long":
                        EmitChecked($"compound[\"{key}\"] = new NbtLong(\"{key}\", {property.Name}{access});");
                        break;
                    case "Float":
                        EmitChecked($"compound[\"{key}\"] = new NbtFloat(\"{key}\", {property.Name}{access});");
                        break;
                    case "Double":
                        EmitChecked($"compound[\"{key}\"] = new NbtDouble(\"{key}\", {property.Name}{access});");
                        break;
                    case "String":
                        EmitChecked($"compound[\"{key}\"] = new NbtString(\"{key}\", {property.Name});");
                        break;
                    case "ByteArray":
                        EmitChecked($"compound[\"{key}\"] = new NbtByteArray(\"{key}\", {property.Name});");
                        break;
                    case "IntArray":
                        EmitChecked($"compound[\"{key}\"] = new NbtIntArray(\"{key}\", {property.Name});");
                        break;
                    case "LongArray":
                        EmitChecked($"compound[\"{key}\"] = new NbtLongArray(\"{key}\", {property.Name});");
                        break;
                    case "NbtArray":
                        var elementType = property.Type is IArrayTypeSymbol at ? at.ElementType : null;

                        bool isNbt = elementType != null && elementType.AllInterfaces.Any(i => i.Name == "INbtComponent");
                        bool isCompound = elementType?.Name == "NbtCompound";
                        bool isString = elementType?.SpecialType == SpecialType.System_String;

                        string code;
                        if (isNbt && !isCompound && !isString)
                        {
                            code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var compoundArray = new INbtTag[length];
        for (int i = 0; i < length; i++)
            compoundArray[i] = {{property.Name}}[i].ToCompound(null);
        compound["{{key}}"] = new NbtList("{{key}}", TagType.Compound, compoundArray);
        """;
                        }
                        else if (isCompound)
                        {
                            code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var compoundArray = new INbtTag[length];
        for (int i = 0; i < length; i++)
            compoundArray[i] = {{property.Name}}[i];
        compound["{{key}}"] = new NbtList("{{key}}", TagType.Compound, compoundArray);
        """;
                        }
                        else
                        {
                            // Strings, numbers, or other primitives
                            code = $$"""
        var length = {{property.Name}}?.Length ?? 0;
        var array = new INbtTag[length];
        for (int i = 0; i < length; i++)
            array[i] = new NbtString("", {{property.Name}}[i].ToString());
        compound["{{key}}"] = new NbtList("{{key}}", TagType.String, array);
        """;
                        }

                        EmitChecked(code);
                        break;
                    case "NbtComponent":
                        EmitChecked($"compound[\"{key}\"] = {property.Name}{access}.ToCompound(\"{key}\");");
                        break;
                    default:
                        sb.AppendLine($"   throw new System.Exception(\"Unsupported Type\");         // Unsupported type: {componentType}");
                        break;
                }
            }
            sb.AppendLine($"            return compound;");
            return sb.ToString();
        }
    }
}