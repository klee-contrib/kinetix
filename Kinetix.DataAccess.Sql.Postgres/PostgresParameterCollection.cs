using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kinetix.DataAccess.Sql.Postgres
{
    /// <summary>
    /// Collection de paramètres pour les commandes Sql Server.
    /// </summary>
    internal class PostgresParameterCollection : SqlParameterCollection
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="command">Commande SQL.</param>
        public PostgresParameterCollection(IDbCommand command)
            : base(command)
        {
        }

        /// <inheritdoc />
        public override SqlDataParameter AddInParameter(string parameterName, IEnumerable<int> list)
        {
            var parameter = new SqlDataParameter(InnerCommand.CreateParameter())
            {
                ParameterName = ParamValue + parameterName,
                Direction = ParameterDirection.Input,
                Value = list.ToList()
            };
            Add(parameter);
            return parameter;
        }

        /// <inheritdoc />
        public override SqlDataParameter AddInParameter(string parameterName, IEnumerable<string> list)
        {
            var parameter = new SqlDataParameter(InnerCommand.CreateParameter())
            {
                ParameterName = ParamValue + parameterName,
                Direction = ParameterDirection.Input,
                Value = list.ToList()
            };
            Add(parameter);
            return parameter;
        }

        /// <inheritdoc />
        public override SqlDataParameter AddTableParameter<T>(ICollection<T> collection)
        {
            var parameter = new PostgresParameterBeanCollection<T>(null, collection, false).CreateParameter(InnerCommand);
            List.Add(parameter);
            return parameter;
        }
    }
}