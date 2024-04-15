using System.Reflection;
using System.Runtime.CompilerServices;

namespace WildcardsUtilities.Scanner.Extensions;

public static class ReflectionExtensions
{
    public static bool HasAttribute<T>(this ICustomAttributeProvider attributeProvider)
        where T : Attribute =>
            attributeProvider.IsDefined(typeof(ExtensionAttribute), false);

    public static IEnumerable<MethodInfo> EnumerateExtensionMethods(this Type type) =>
        AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Where(HasAttribute<ExtensionAttribute>)
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(HasAttribute<ExtensionAttribute>)
            .SelectMany(type => type.DeclaredMethods)
            .Where
            (
                method =>
                    method.HasAttribute<ExtensionAttribute>() &&
                    method.GetParameters()[0].ParameterType == type
            );
}
