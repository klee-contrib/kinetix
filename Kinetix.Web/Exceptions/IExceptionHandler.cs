﻿using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler d'exception pour les filtres ASP.NET Core.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Priorité du handler. Plus le nombre est grand, plus le handler sera prioritaire dans le traitement des exceptions.
    /// Les handlers par défaut ont une priorité de 1.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gère une exception.
    /// </summary>
    /// <param name="exception">Exception.</param>
    /// <returns>Résultat si traité, null si ignoré.</returns>
    IResult Handle(Exception exception);
}

