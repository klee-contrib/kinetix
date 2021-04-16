namespace Kinetix.Edm
{
    public interface IEdmManager
    {
        IEdmStore GetStore(string dataSourceName = null);
    }
}