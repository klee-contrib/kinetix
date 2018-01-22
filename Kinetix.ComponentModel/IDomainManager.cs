namespace Kinetix.ComponentModel
{
    public interface IDomainManager
    {
        IDomainChecker GetDomain(BeanPropertyDescriptor property);
        IDomain GetDomain(string domainName);
    }
}