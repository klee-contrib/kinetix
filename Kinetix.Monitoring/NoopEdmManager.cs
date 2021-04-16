namespace Kinetix.Edm
{
    public class NoopEdmManager : IEdmManager
    {
        public IEdmStore GetStore(string dataSourceName = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
