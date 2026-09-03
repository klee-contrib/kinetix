using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Kinetix.Search.Core.DocumentModel;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Builder de requête ElasticSearch.
/// </summary>
public static class ElasticQueryBuilder
{
    /// <summary>
    /// Construit une requête avec des AND (filter) sur des sous-requêtes.
    /// </summary>
    /// <param name="subQueries">Sous-requêtes.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildAndQuery<TDocument>(
        params Action<QueryDescriptor<TDocument>>[] subQueries
    )
        where TDocument : class
    {
        return subQueries.Length switch
        {
            0 => q => { },
            1 => subQueries[0],
            _ => q => q.Bool(b => b.Filter(subQueries)),
        };
    }

    /// <summary>
    /// Construit une requête pour le filtrage exacte (sélection de facette, ...).
    /// </summary>
    /// <param name="field">Champ.</param>
    /// <param name="value">Valeur.</param>
    /// <param name="invert">Inverse le filtre.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildFilter<TDocument>(
        string field,
        string value,
        bool invert = false
    )
        where TDocument : class
    {
        void Query(QueryDescriptor<TDocument> q) => q.Term(t => t.Field(field).Value(value));

        return invert ? (q => q.Bool(b => b.MustNot(Query))) : Query;
    }

    /// <summary>
    /// Construit une requête pour un champ qui doit manquer (équivalent d'une valeur NULL).
    /// </summary>
    /// <param name="field">Champ.</param>
    /// <param name="invert">Inverse le filtre.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildMissingField<TDocument>(string field, bool invert = false)
        where TDocument : class
    {
        void Query(QueryDescriptor<TDocument> q) => q.Exists(t => t.Field(field));

        return invert ? Query : (q => q.Bool(b => b.MustNot(Query)));
    }

    /// <summary>
    /// Construit une requête pour une recherche textuelle.
    /// </summary>
    /// <param name="text">Texte de recherche.</param>
    /// <param name="fields">Champs de recherche.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildMultiMatchQuery<TDocument>(
        string text,
        params DocumentFieldDescriptor[] fields
    )
        where TDocument : class
    {
        return q =>
            q.MultiMatch(m =>
                m.Query(text)
                    .Operator(Operator.And)
                    .Type(TextQueryType.CrossFields)
                    .Fields(fields.Select(f => new Field(f.FieldName, f.Boost)).ToArray())
            );
    }

    /// <summary>
    /// Construit une requête avec des AND (must, pour calculer le score) sur des sous-requêtes.
    /// </summary>
    /// <param name="subQueries">Sous-requêtes.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildMustQuery<TDocument>(
        params Action<QueryDescriptor<TDocument>>[] subQueries
    )
        where TDocument : class
    {
        return subQueries.Length switch
        {
            0 => q => { },
            1 => subQueries[0],
            _ => q => q.Bool(b => b.Must(subQueries)),
        };
    }

    /// <summary>
    /// Construit une requête avec des OR sur des sous-requêtes.
    /// </summary>
    /// <param name="subQueries">Sous-requêtes.</param>
    /// <returns>Requête.</returns>
    public static Action<QueryDescriptor<TDocument>> BuildOrQuery<TDocument>(
        params Action<QueryDescriptor<TDocument>>[] subQueries
    )
        where TDocument : class
    {
        return subQueries.Length switch
        {
            0 => q => { },
            1 => subQueries[0],
            _ => q => q.Bool(b => b.Should(subQueries).MinimumShouldMatch(1)),
        };
    }
}
