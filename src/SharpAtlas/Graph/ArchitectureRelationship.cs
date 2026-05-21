namespace SharpAtlas.Graph;

public static class ArchitectureRelationship
{
    public const string Constructor = "constructor";
    public const string Field = "field";
    public const string Property = "property";
    public const string MethodParameter = "method-parameter";
    public const string MethodReturn = "method-return";
    public const string Inherits = "inherits";
    public const string Implements = "implements";
    public const string Generic = "generic";
    public const string RecordParameter = "record-parameter";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Constructor,
        Field,
        Property,
        MethodParameter,
        MethodReturn,
        Inherits,
        Implements,
        Generic,
        RecordParameter
    };
}
