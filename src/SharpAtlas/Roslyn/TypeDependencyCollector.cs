using Microsoft.CodeAnalysis;
using SharpAtlas.Graph;

namespace SharpAtlas.Roslyn;

public static class TypeDependencyCollector
{
    public static IReadOnlyList<TypeDependency> Collect(INamedTypeSymbol sourceType)
    {
        var dependencies = new List<TypeDependency>();

        if (sourceType.BaseType is { SpecialType: not SpecialType.System_Object } baseType)
        {
            AddTypeDependency(dependencies, baseType, ArchitectureRelationship.Inherits);
        }

        foreach (var interfaceType in sourceType.Interfaces)
        {
            AddTypeDependency(dependencies, interfaceType, ArchitectureRelationship.Implements);
        }

        foreach (var member in sourceType.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol field when !field.IsImplicitlyDeclared:
                    AddTypeDependency(dependencies, field.Type, ArchitectureRelationship.Field);
                    break;
                case IPropertySymbol property when !property.IsImplicitlyDeclared:
                    AddTypeDependency(dependencies, property.Type, ArchitectureRelationship.Property);
                    break;
                case IMethodSymbol method:
                    AddMethodDependencies(dependencies, sourceType, method);
                    break;
            }
        }

        return dependencies;
    }

    private static void AddMethodDependencies(List<TypeDependency> dependencies, INamedTypeSymbol sourceType, IMethodSymbol method)
    {
        if (method.IsImplicitlyDeclared)
        {
            return;
        }

        if (method.MethodKind == MethodKind.Constructor)
        {
            foreach (var parameter in method.Parameters)
            {
                AddTypeDependency(dependencies, parameter.Type, ArchitectureRelationship.Constructor);
                if (sourceType.IsRecord)
                {
                    AddTypeDependency(dependencies, parameter.Type, ArchitectureRelationship.RecordParameter);
                }
            }

            return;
        }

        if (method.MethodKind != MethodKind.Ordinary)
        {
            return;
        }

        foreach (var parameter in method.Parameters)
        {
            AddTypeDependency(dependencies, parameter.Type, ArchitectureRelationship.MethodParameter);
        }

        if (!method.ReturnsVoid)
        {
            AddTypeDependency(dependencies, method.ReturnType, ArchitectureRelationship.MethodReturn);
        }
    }

    private static void AddTypeDependency(List<TypeDependency> dependencies, ITypeSymbol type, string relationship)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            AddTypeDependency(dependencies, arrayType.ElementType, relationship);
            return;
        }

        if (type is IPointerTypeSymbol pointerType)
        {
            AddTypeDependency(dependencies, pointerType.PointedAtType, relationship);
            return;
        }

        if (type is INamedTypeSymbol namedType)
        {
            var definition = namedType.OriginalDefinition;
            if (definition.TypeKind != TypeKind.TypeParameter)
            {
                dependencies.Add(new TypeDependency(definition, relationship));
            }

            foreach (var typeArgument in namedType.TypeArguments)
            {
                AddTypeDependency(dependencies, typeArgument, ArchitectureRelationship.Generic);
            }
        }
    }
}
