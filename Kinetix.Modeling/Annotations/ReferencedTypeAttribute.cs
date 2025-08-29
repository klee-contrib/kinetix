namespace Kinetix.Modeling.Annotations;

/// <summary>
/// Attribut de description de typage d'une association.
/// </summary>
/// <remarks>
/// Crée une nouvelle instance.
/// </remarks>
/// <param name="referenceType">Type référencé.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ReferencedTypeAttribute(Type referenceType) : Attribute
{
    /// <summary>
    /// Obtient le type de l'objet de référence associé à la propriété.
    /// </summary>
    public Type ReferenceType { get; private set; } = referenceType ?? throw new ArgumentNullException("referenceType");
}
