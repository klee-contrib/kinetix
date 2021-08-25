using Kinetix.Services;

namespace Kinetix.Data.SqlClient
{
    public class SqlTransactionContextProvider : ITransactionContextProvider
    {
        public ITransactionContext Create()
        {
            return new SqlTransactionContext();
        }
    }
}
