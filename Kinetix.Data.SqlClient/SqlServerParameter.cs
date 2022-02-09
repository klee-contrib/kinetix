using System;
using System.Data;
using System.Data.SqlClient;

namespace Kinetix.Data.SqlClient
{
    /// <summary>
    /// Paramètre d'appel d'une commande Sql Server.
    /// </summary>
    public sealed class SqlServerParameter : IDbDataParameter
    {
        /// <summary>
        /// Crée un nouveau paramétre.
        /// </summary>
        /// <param name="parameter">Paramétre interne.</param>
        internal SqlServerParameter(IDbDataParameter parameter)
        {
            InnerParameter = parameter;
        }

        /// <summary>
        /// Obtient ou définit le type SQL Server dédié.
        /// </summary>
        public SqlDbType SqlDbType
        {
            get => ((SqlParameter)InnerParameter).SqlDbType;
            set => ((SqlParameter)InnerParameter).SqlDbType = value;
        }

        /// <summary>
        /// Obtient ou définit le nom du type dédié.
        /// </summary>
        public string TypeName
        {
            get => ((SqlParameter)InnerParameter).TypeName;
            set => ((SqlParameter)InnerParameter).TypeName = value;
        }

        /// <summary>
        /// Obtient ou définit le nom du type dédié.
        /// </summary>
        public string UdtTypeName
        {
            get => ((SqlParameter)InnerParameter).UdtTypeName;
            set => ((SqlParameter)InnerParameter).UdtTypeName = value;
        }

        /// <summary>
        /// Obtient ou définit le type de données.
        /// </summary>
        public DbType DbType
        {
            get => InnerParameter.DbType;
            set => InnerParameter.DbType = value;
        }

        /// <summary>
        /// Obtient ou définit la direction du paramètre.
        /// </summary>
        public ParameterDirection Direction
        {
            get => InnerParameter.Direction;
            set => InnerParameter.Direction = value;
        }

        /// <summary>
        /// Obtient ou définit le nom du paramétre.
        /// </summary>
        public string ParameterName
        {
            get => InnerParameter.ParameterName;
            set => InnerParameter.ParameterName = value;
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Precision
        {
            get => InnerParameter.Precision;
            set => InnerParameter.Precision = value;
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Scale
        {
            get => InnerParameter.Scale;
            set => InnerParameter.Scale = value;
        }

        /// <summary>
        /// Obtient ou définit la taille.
        /// </summary>
        public int Size
        {
            get => InnerParameter.Size;
            set => InnerParameter.Size = value;
        }

        /// <summary>
        /// Obtient ou définit la valeur du paramètre.
        /// La valeur peut être nulle.
        /// </summary>
        public object Value
        {
            get
            {
                var val = InnerParameter.Value;
                return DBNull.Value.Equals(val) ? null : val;
            }

            set => InnerParameter.Value = value ?? DBNull.Value;
        }

        /// <summary>
        /// Obtient ou définit la colonne source.
        /// </summary>
        string IDataParameter.SourceColumn
        {
            get => InnerParameter.SourceColumn;
            set => InnerParameter.SourceColumn = value;
        }

        /// <summary>
        /// Obtient ou définit la version de la colonne source.
        /// </summary>
        DataRowVersion IDataParameter.SourceVersion
        {
            get => InnerParameter.SourceVersion;
            set => InnerParameter.SourceVersion = value;
        }

        /// <summary>
        /// Indique si le paramètre est nullable.
        /// </summary>
        bool IDataParameter.IsNullable => true;

        /// <summary>
        /// Retourne le paramètre interne.
        /// </summary>
        internal IDbDataParameter InnerParameter { get; }
    }
}
