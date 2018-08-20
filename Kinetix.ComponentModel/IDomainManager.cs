namespace Kinetix.ComponentModel
{
    public interface IDomainManager
    {
        /// <summary>
        /// Retourne le domaine associé à une propriété.
        /// </summary>
        /// <param name="property">Description de la propriété.</param>
        /// <returns>Null si aucun domaine n'est associé.</returns>
        IDomainChecker GetDomain(BeanPropertyDescriptor property);

        /// <summary>
        /// Retourne la description d'un domaine.
        /// </summary>
        /// <param name="domainName">Nom du domaine.</param>
        /// <returns>Domaine.</returns>
        /// <exception cref="System.NotSupportedException">Si le domaine n'est pas connu.</exception>
        IDomain GetDomain(string domainName);
    }
}