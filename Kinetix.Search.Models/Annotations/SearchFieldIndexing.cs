﻿namespace Kinetix.Search.Models.Annotations;

/// <summary>
/// Type d'indexage pour le moteur de recherche.
/// </summary>
public enum SearchFieldIndexing
{
    /// <summary>
    /// Le champ n'est pas indexé.
    /// </summary>
    None,

    /// <summary>
    /// Champ de recherche : indexé tokenisé en minuscule.
    /// </summary>
    FullText,

    /// <summary>
    /// Champ de tri : indexé en lowercase/asciifolding.
    /// </summary>
    Sort,

    /// <summary>
    /// Champ de facette : indexé tel quel.
    /// </summary>
    Term
}
