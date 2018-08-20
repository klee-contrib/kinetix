using AutoMapper;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Moteur de beans.
    ///
    /// Publie des méthodes pour :
    /// - copier un bean dans un autre
    /// - cloner un bean.
    /// </summary>
    public static class BeanEngine
    {
        /// <summary>
        /// Recopie un bean source dans un bean destination.
        /// </summary>
        /// <typeparam name="TSource">Type du bean source.</typeparam>
        /// <typeparam name="TDestination">Type du bean destination.</typeparam>
        /// <param name="source">Bean source.</param>
        /// <returns>Bean destination.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            /* Vérifie que le mapping existe. */
            var mapper = GetMapper<TSource, TDestination>();

            /* Exécute le mapping. */
            var destination = mapper.Map<TSource, TDestination>(source);

            return destination;
        }

        /// <summary>
        /// Recopie un bean source dans un bean destination.
        /// </summary>
        /// <typeparam name="TSource">Type du bean source.</typeparam>
        /// <typeparam name="TDestination">Type du bean destination.</typeparam>
        /// <param name="source">Bean source.</param>
        /// <param name="destination">Bean destination.</param>
        /// <returns>Bean destination.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            /* Vérifie que le mapping existe. */
            var mapper = GetMapper<TSource, TDestination>();

            /* Exécute le mapping. */
            mapper.Map(source, destination);

            return destination;
        }

        /// <summary>
        /// Create a mapper for a given tuple object.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <returns>Mapper.</returns>
        public static IMapper GetMapper<TSource, TDestination>()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TSource, TDestination>());
            var mapper = config.CreateMapper();

            return mapper;
        }
    }
}
