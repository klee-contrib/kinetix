namespace Kinetix.Search.Elastic.Mapping;

/// <summary>
/// Permet de préciser un mapping personnalisé pour un champ.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ElasticMapperAttribute : Attribute
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="mapperType">Mapper.</param>
    public ElasticMapperAttribute(Type mapperType)
    {
        MapperType = mapperType;
    }

    public Type MapperType { get; }
}
