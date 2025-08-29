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
public class TermFacet<TDocument>(string code, string label, Expression<Func<TDocument, object>> field) : IFacetDefinition<TDocument>
{
    /// <inheritdoc />
    public string Code { get; } = code;

    /// <inheritdoc />
    public string Label { get; } = label;

    /// <inheritdoc />
    public Expression<Func<TDocument, object>> Field { get; } = field;

    /// <inheritdoc />
    public string FieldName => Field.Body switch
    {
        UnaryExpression ue => HandleMember((MemberExpression)ue.Operand),
        MemberExpression me => HandleMember(me),
        _ => throw new ArgumentException("Incorrect facet field definition.")
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

    private string HandleMember(MemberExpression me)
    {
        var name = ToCamelCase(me.Member.Name);

        while (me.Expression is MethodCallExpression or MemberExpression)
        {
            me = me.Expression is MethodCallExpression mce ? (MemberExpression)mce.Arguments[0] : (MemberExpression)me.Expression;
            name = $"{ToCamelCase(me.Member.Name)}.{name}";
        }

        return name;
    }

    private string ToCamelCase(string text)
    {
        return char.ToLower(text[0]) + text.Substring(1);
    }
}
