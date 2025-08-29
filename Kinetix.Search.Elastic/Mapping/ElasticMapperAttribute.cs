namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Permet de préciser un mapping personnalisé pour un champ.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="mapperType">Mapper.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ElasticMapperAttribute(Type mapperType) : Attribute
{
    public Type MapperType { get; } = mapperType;
}
