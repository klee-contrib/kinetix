using System.Collections;
using System.Data;
using Kinetix.Modeling;

namespace Kinetix.DataAccess.Sql.Common;

/// <summary>
/// Collection de paramètres pour les commandes Sql Server.
/// </summary>
/// <remarks>
/// Crée une nouvelle instance.
/// </remarks>
/// <param name="command">Commande.</param>
public abstract class SqlParameterCollection(IDbCommand command) : IDataParameterCollection, IList<SqlDataParameter>
{
    /// <summary>
    /// Mot-clés de définition des paramètres dans les
    /// requêtes SQL Server.
    /// </summary>
    public const string ParamValue = "@";

    private readonly IDataParameterCollection _innerCollection = command.Parameters;

    /// <summary>
    /// Retourne le nombre d'éléments de la collection.
    /// </summary>
    public int Count => List.Count;

    /// <summary>
    /// Indique si la collection a une taille fixe.
    /// </summary>
    public bool IsFixedSize => _innerCollection.IsFixedSize;

    /// <summary>
    /// Indique si la collection est en lecture seule.
    /// </summary>
    public bool IsReadOnly => _innerCollection.IsReadOnly;

    /// <summary>
    /// Indique si la collection est synchronisée.
    /// </summary>
    public bool IsSynchronized => _innerCollection.IsSynchronized;

    /// <summary>
    /// Retourne le point de synchronisation pour la collection.
    /// </summary>
    public object SyncRoot => _innerCollection.SyncRoot;

    /// <summary>
    /// Commande SQL.
    /// </summary>
    protected IDbCommand InnerCommand { get; } = command;

    /// <summary>
    /// Liste des paramètres.
    /// </summary>
    protected IList<SqlDataParameter> List { get; } = [];

    /// <summary>
    /// Obtient ou définit un paramètre de la collection.
    /// </summary>
    /// <param name="index">Numéro du paramètre.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter this[int index]
    {
        get => List[index];
        set
        {
            List[index] = value ?? throw new ArgumentNullException(nameof(value));
            _innerCollection[index] = value.InnerParameter;
        }
    }

    /// <summary>
    /// Obtient ou définit un paramètre de la collection.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter this[string parameterName]
    {
        get
        {
            var index = IndexOf(parameterName);
            return List[index];
        }
        set
        {
            var index = IndexOf(parameterName);
            List[index] = value ?? throw new ArgumentNullException(nameof(value));
            _innerCollection[index] = value.InnerParameter;
        }
    }

    /// <summary>
    /// Obtient ou définit un paramètre de la collection.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <returns>Paramètre.</returns>
    object IDataParameterCollection.this[string parameterName]
    {
        get => this[parameterName];
        set => this[parameterName] = (SqlDataParameter)value;
    }

    /// <summary>
    /// Obtient ou définit un paramètre de la collection.
    /// </summary>
    /// <param name="index">Numéro du paramètre.</param>
    /// <returns>Paramètre.</returns>
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (SqlDataParameter)value!;
    }

    /// <summary>
    /// Ajoute un paramètre la collection.
    /// </summary>
    /// <param name="parameter">Nouveau paramètre.</param>
    /// <returns>Paramètre ajouté.</returns>
    public SqlDataParameter Add(SqlDataParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        List.Add(parameter);
        _innerCollection.Add(parameter.InnerParameter);
        return parameter;
    }

    /// <summary>
    /// Ajoute un paramètre la collection.
    /// </summary>
    /// <param name="value">Nouveau paramètre.</param>
    /// <returns>Indice d'ajout.</returns>
    int IList.Add(object? value)
    {
        Add((SqlDataParameter)value!);
        return List.Count - 1;
    }

    /// <summary>
    /// Ajoute un paramètre la collection.
    /// </summary>
    /// <param name="parameter">Nouveau paramètre.</param>
    void ICollection<SqlDataParameter>.Add(SqlDataParameter parameter)
    {
        Add(parameter);
    }

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des entiers.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des entiers à insérer dans le IN.</param>
    /// <returns>Le paramètre créé.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public abstract SqlDataParameter AddInParameter(string parameterName, IEnumerable<int> list);

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des chaines de caractères.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des strings à insérer dans le IN.</param>
    /// <returns>Le paramètre créé.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public abstract SqlDataParameter AddInParameter(string parameterName, IEnumerable<string> list);

    /// <summary>
    /// Ajoute les paramètres pour une clause IN portant sur des chaines de caractères.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre SQL Server.</param>
    /// <param name="list">Collection des guids à insérer dans le IN.</param>
    /// <returns>Le paramètre créé.</returns>
    /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
    public abstract SqlDataParameter AddInParameter(string parameterName, IEnumerable<Guid> list);

    /// <summary>
    /// Ajoute une liste de bean en paramètre (La colonne InsertKey est obligatoire).
    /// </summary>
    /// <typeparam name="T">Type du bean.</typeparam>
    /// <param name="collection">Collection à passer en paramètre.</param>
    /// <returns>Parameter.</returns>
    public abstract SqlDataParameter AddTableParameter<T>(ICollection<T> collection)
        where T : class, new();

    /// <summary>
    /// Ajout un nouveau paramètre à partir de son nom et de sa valeur.
    /// Le paramètre est un paramètre d'entrée.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <param name="value">Valeur du paramètre.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter AddWithValue(string parameterName, object? value)
    {
        var param = InnerCommand.CreateParameter();
        param.ParameterName = ParamValue + parameterName;

        if (value == null)
        {
            param.Value = DBNull.Value;
        }
        else
        {
            var t = value.GetType();
            if (!SetDbType(param, t))
            {
                throw new NotSupportedException("La gestion du type " + t.Name + " doit être implémentée.");
            }

            param.Value = value;
        }

        _innerCollection.Add(param);

        var p = new SqlDataParameter(param);
        List.Add(p);
        return p;
    }

    /// <summary>
    /// Ajout un nouveau paramètre à partir d'une colonne et de sa valeur.
    /// Le paramètre est un paramètre d'entrée.
    /// </summary>
    /// <param name="colName">Colonnne du paramètre.</param>
    /// <param name="value">Valeur du paramètre.</param>
    /// <returns>Paramètre.</returns>
    public SqlDataParameter AddWithValue(Enum colName, object value)
    {
        ArgumentNullException.ThrowIfNull(colName);

        return AddWithValue(colName.ToString(), value);
    }

    /// <summary>
    /// Efface tous paramètres.
    /// </summary>
    public void Clear()
    {
        List.Clear();
        _innerCollection.Clear();
    }

    /// <summary>
    /// Indique si la collection contient un paramètre.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <returns>True si la collection contient le paramètre.</returns>
    public bool Contains(string parameterName)
    {
        return _innerCollection.Contains(parameterName);
    }

    /// <summary>
    /// Indique si la collection contient un paramètre.
    /// </summary>
    /// <param name="item">Paramètre.</param>
    /// <returns>True si la collection contient le paramètre.</returns>
    public bool Contains(SqlDataParameter item)
    {
        return List.Contains(item);
    }

    /// <summary>
    /// Indique si la collection contient un paramètre.
    /// </summary>
    /// <param name="value">Paramètre.</param>
    /// <returns>True si la collection contient le paramètre.</returns>
    bool IList.Contains(object? value)
    {
        return List.Contains((SqlDataParameter)value!);
    }

    /// <summary>
    /// Copie la collection dans un tableau d'objet.
    /// Cette méthode n'est pas supportée.
    /// </summary>
    /// <param name="array">Tableau de sortie.</param>
    /// <param name="arrayIndex">Index de début de copie.</param>
    public void CopyTo(SqlDataParameter[] array, int arrayIndex)
    {
        List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Copie la collection dans un tableau d'objet.
    /// Cette méthode n'est pas supportée.
    /// </summary>
    /// <param name="array">Tableau de sortie.</param>
    /// <param name="index">Index de début de copie.</param>
    void ICollection.CopyTo(Array array, int index)
    {
        ((IList)List).CopyTo(array, index);
    }

    /// <summary>
    /// Retourne un enumérateur sur la collection.
    /// </summary>
    /// <returns>Enumerateur.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return List.GetEnumerator();
    }

    /// <summary>
    /// Retourne un enumérateur sur la collection.
    /// </summary>
    /// <returns>Enumerateur.</returns>
    IEnumerator<SqlDataParameter> IEnumerable<SqlDataParameter>.GetEnumerator()
    {
        return List.GetEnumerator();
    }

    /// <summary>
    /// Retourne la position d'un paramètre dans la collection.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
    public int IndexOf(string parameterName)
    {
        return _innerCollection.IndexOf(parameterName);
    }

    /// <summary>
    /// Retourne la position d'un paramètre dans la collection.
    /// </summary>
    /// <param name="item">Paramètre.</param>
    /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
    public int IndexOf(SqlDataParameter item)
    {
        return List.IndexOf(item);
    }

    /// <summary>
    /// Retourne la position d'un paramètre dans la collection.
    /// </summary>
    /// <param name="value">Paramètre.</param>
    /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
    int IList.IndexOf(object? value)
    {
        return IndexOf((SqlDataParameter)value!);
    }

    /// <summary>
    /// Ajoute un paramètre à la collection.
    /// </summary>
    /// <param name="index">Index d'insertion (0 pour insérer en première position).</param>
    /// <param name="item">Paramètre.</param>
    public void Insert(int index, SqlDataParameter item)
    {
        ArgumentNullException.ThrowIfNull(item);

        List.Insert(index, item);
        _innerCollection.Insert(index, item.InnerParameter);
    }

    /// <summary>
    /// Ajoute un paramètre à la collection.
    /// </summary>
    /// <param name="index">Index d'insertion (0 pour insérer en première position).</param>
    /// <param name="value">Paramètre.</param>
    void IList.Insert(int index, object? value)
    {
        Insert(index, (SqlDataParameter)value!);
    }

    /// <summary>
    /// Supprime un paramètre de la collection.
    /// </summary>
    /// <param name="item">Paramètre.</param>
    /// <returns>True si le paramètre a été supprimé.</returns>
    public bool Remove(SqlDataParameter item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var isRemoved = List.Remove(item);
        _innerCollection.Remove(item.InnerParameter);
        return isRemoved;
    }

    /// <summary>
    /// Supprime un paramètre de la collection.
    /// </summary>
    /// <param name="value">Paramètre.</param>
    void IList.Remove(object? value)
    {
        Remove((SqlDataParameter)value!);
    }

    /// <summary>
    /// Supprime un paramètre de la collection.
    /// </summary>
    /// <param name="parameterName">Nom du paramètre.</param>
    public void RemoveAt(string parameterName)
    {
        var index = IndexOf(parameterName);
        RemoveAt(index);
    }

    /// <summary>
    /// Supprime un paramètre de la collection.
    /// </summary>
    /// <param name="index">Indice du paramètre.</param>
    public void RemoveAt(int index)
    {
        _innerCollection.RemoveAt(index);
        List.RemoveAt(index);
    }

    protected virtual bool SetDbType(IDbDataParameter param, Type t)
    {
        if (t == typeof(string))
        {
            param.DbType = DbType.String;
            return true;
        }
        else if (t == typeof(byte))
        {
            param.DbType = DbType.Byte;
            return true;
        }
        else if (t == typeof(short))
        {
            param.DbType = DbType.Int16;
            return true;
        }
        else if (t == typeof(int))
        {
            param.DbType = DbType.Int32;
            return true;
        }
        else if (t == typeof(long))
        {
            param.DbType = DbType.Int64;
            return true;
        }
        else if (t == typeof(decimal))
        {
            param.DbType = DbType.Decimal;
            return true;
        }
        else if (t == typeof(float))
        {
            param.DbType = DbType.Single;
            return true;
        }
        else if (t == typeof(double))
        {
            param.DbType = DbType.Double;
            return true;
        }
        else if (t == typeof(Guid))
        {
            param.DbType = DbType.Guid;
            return true;
        }
        else if (t == typeof(bool))
        {
            param.DbType = DbType.Boolean;
            return true;
        }
        else if (t == typeof(byte[]))
        {
            param.DbType = DbType.Binary;
            return true;
        }
        else if (t == typeof(DateTime))
        {
            param.DbType = DbType.DateTime;
            return true;
        }
        else if (t == typeof(System.Data.SqlTypes.SqlDateTime))
        {
            param.DbType = DbType.DateTime2;
            return true;
        }
        else if (t == typeof(ChangeAction))
        {
            param.DbType = DbType.String;
            return true;
        }
        else if (t == typeof(char))
        {
            param.DbType = DbType.String;
            return true;
        }

        return false;
    }
}
