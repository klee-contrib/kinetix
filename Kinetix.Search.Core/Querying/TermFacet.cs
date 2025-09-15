using System.Linq.Expressions;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette simple.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="code">Code de la facette.</param>
/// <param name="label">Libellé de la facette.</param>
/// <param name="field">Champ sur lequel agit la facette.</param>
public class TermFacet<TDocument>(string code, string label, Expression<Func<TDocument, object>> field)
    : IFacetDefinition<TDocument>
{
    /// <inheritdoc />
    public string Code { get; } = code;

    /// <inheritdoc />
    public string Label { get; } = label;

    /// <inheritdoc />
    public Expression<Func<TDocument, object>> Field { get; } = field;

    /// <inheritdoc />
    public string FieldName =>
        Field.Body switch
        {
            UnaryExpression ue => TermFacet<TDocument>.HandleMember((MemberExpression)ue.Operand),
            MemberExpression me => TermFacet<TDocument>.HandleMember(me),
            _ => throw new Exception("Incorrect facet field definition."),
        };

    /// <inheritdoc />
    public virtual bool IsMultiSelectable { get; set; } = false;

    /// <inheritdoc />
    public virtual bool CanExclude { get; set; } = false;

    /// <inheritdoc />
    public virtual bool HasMissing { get; set; } = true;

    /// <inheritdoc />
    public virtual FacetOrdering Ordering { get; set; } = FacetOrdering.CountDescending;

    /// <inheritdoc cref="IFacetDefinition{TDocument}.ResolveLabel" />
    public virtual string ResolveLabel(string primaryKey)
    {
        return primaryKey;
    }

    private static string HandleMember(MemberExpression me)
    {
        var name = TermFacet<TDocument>.ToCamelCase(me.Member.Name);

        while (me.Expression is MethodCallExpression or MemberExpression)
        {
            me = me.Expression is MethodCallExpression mce
                ? (MemberExpression)mce.Arguments[0]
                : (MemberExpression)me.Expression;
            name = $"{TermFacet<TDocument>.ToCamelCase(me.Member.Name)}.{name}";
        }

        return name;
    }

    private static string ToCamelCase(string text)
    {
        return char.ToLower(text[0]) + text[1..];
    }
}
