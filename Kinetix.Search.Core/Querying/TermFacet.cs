using System.Linq.Expressions;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette simple.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public class TermFacet<TDocument> : IFacetDefinition<TDocument>
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    public TermFacet(string code, string label, Expression<Func<TDocument, object>> field)
    {
        Code = code;
        Label = label;
        Field = field;
    }

    /// <inheritdoc cref="IFacetDefinition.Code" />
    public string Code { get; }

    /// <inheritdoc cref="IFacetDefinition.Label" />
    public string Label { get; }

    /// <inheritdoc cref="IFacetDefinition.Field" />
    public Expression<Func<TDocument, object>> Field { get; }

    /// <inheritdoc cref="IFacetDefinition.FieldName" />
    public string FieldName => Field.Body switch
    {
        UnaryExpression ue => HandleMember((MemberExpression)ue.Operand),
        MemberExpression me => HandleMember(me),
        _ => throw new ArgumentException("Incorrect facet field definition.")
    };

    /// <inheritdoc cref="IFacetDefinition.IsMultiSelectable" />
    public virtual bool IsMultiSelectable { get; set; } = false;

    /// <inheritdoc cref="IFacetDefinition.CanExclude" />
    public virtual bool CanExclude { get; set; } = false;

    /// <inheritdoc cref="IFacetDefinition.HasMissing" />
    public virtual bool HasMissing { get; set; } = true;

    /// <inheritdoc cref="IFacetDefinition.Ordering" />
    public virtual FacetOrdering Ordering { get; set; } = FacetOrdering.CountDescending;

    /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
    public virtual string ResolveLabel(string primaryKey)
    {
        return primaryKey;
    }

    private string HandleMember(MemberExpression me)
    {
        var name = ToCamelCase(me.Member.Name);

        if (me.Expression is MethodCallExpression mce)
        {
            name = $"{ToCamelCase(((MemberExpression)mce.Arguments[0]).Member.Name)}.{name}";
        }

        return name;
    }

    private string ToCamelCase(string text)
    {
        return char.ToLower(text[0]) + text.Substring(1);
    }
}
