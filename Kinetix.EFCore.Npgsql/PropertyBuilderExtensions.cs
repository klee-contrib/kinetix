using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore;

public static class PropertyBuilderExtensions
{
    /// <summary>
    /// Surcharge de 'UseIdentityColumn' du provider PostgreSQL pour avoir la même API que le provider SQL Server.
    /// </summary>
    /// <param name="propertyBuilder">Le builder de propriété.</param>
    /// <param name="startValue">Valeur initiale de la séquence.</param>
    /// <param name="incrementBy">Incrément de la séquence.</param>
    /// <returns>Le même builder de propriété.</returns>
    public static PropertyBuilder UseIdentityColumn(
        this PropertyBuilder propertyBuilder,
        long startValue,
        long incrementBy
    )
    {
        return propertyBuilder.UseIdentityByDefaultColumn().HasIdentityOptions(startValue, incrementBy);
    }
}
