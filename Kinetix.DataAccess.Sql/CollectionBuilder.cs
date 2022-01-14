using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Kinetix.DataAccess.Sql
{
    /// <summary>
    /// Constructeur de collection.
    /// </summary>
    /// <typeparam name="T">Type de collection à construire.</typeparam>
    public static class CollectionBuilder<T>
        where T : class, new()
    {
        /// <summary>
        /// Parse un DataReader et énumère les éléments.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <returns>Les éléments.</returns>
        public static IEnumerable<T> ParseCommand(BaseSqlCommand cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            if (cmd.QueryParameters != null)
            {
                cmd.QueryParameters.RemapSortColumn(typeof(T));
            }

            using var reader = cmd.ExecuteReader();
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            IDataRecordAdapter<T> adapter = null;

            while (reader.Read())
            {
                if (adapter == null)
                {
                    adapter = DataRecordAdapterManager<T>.CreateAdapter(reader);
                }

                yield return adapter.Read(null, reader);
            }
        }

        /// <summary>
        /// Lit un objet dans le reader.
        /// </summary>
        /// <param name="cmd">Commande a executer.</param>
        /// <param name="returnNullIfZeroRow">Indique si une valeur nulle doit être retournée si il n'y a aucune ligne.</param>
        /// <returns>Objet.</returns>
        public static T ParseCommandForSingleObject(BaseSqlCommand cmd, bool returnNullIfZeroRow = false)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }

            try
            {
                using var reader = cmd.ExecuteReader();

                IDataRecordAdapter<T> adapter = null;
                T destination = null;

                while (reader.Read())
                {
                    adapter = adapter == null
                        ? DataRecordAdapterManager<T>.CreateAdapter(reader)
                        : throw new CollectionBuilderException("Too many rows selected !");

                    destination = adapter.Read(destination, reader);
                }

                return adapter == null && returnNullIfZeroRow
                    ? default
                    : destination;
            }
            catch (NotSupportedException e)
            {
                var parameters = new StringBuilder();
                foreach (IDataParameter parameter in cmd.Parameters)
                {
                    parameters.AppendFormat("{0} = {1}, ", parameter.ParameterName, parameter.Value);
                }

                throw new CollectionBuilderException("Paramètres de la commande: " + parameters, e);
            }
        }
    }
}
