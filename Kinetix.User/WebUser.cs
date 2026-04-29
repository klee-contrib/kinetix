using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Kinetix.User;

/// <summary>
/// Implémentation de ICurrentUser via HttpContext.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="httpContext">HttpContext.</param>
public class WebUser(HttpContext httpContext) : ICurrentUser
{
    /// <inheritdoc cref="ICurrentUser.Login" />
    public string? Login => httpContext.User.Identity?.Name;

    /// <inheritdoc cref="ICurrentUser.Roles" />
    public IEnumerable<string> Roles =>
        httpContext.User.Identity is not ClaimsIdentity identity
            ? []
            : identity
                .FindAll(identity.RoleClaimType)
                .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                .Select(c => c.Value);

    /// <inheritdoc cref="ICurrentUser.GetString" />
    public string? GetString(string claimType)
    {
        if (httpContext.User.Identity is not ClaimsIdentity identity)
        {
            return null;
        }

        var claim = identity.FindFirst(claimType);
        return claim?.Value;
    }

    /// <inheritdoc cref="ICurrentUser.GetStrings" />
    public IEnumerable<string> GetStrings(string claimType)
    {
        return httpContext.User.Identity is not ClaimsIdentity identity
            ? []
            : identity.FindAll(claimType).Select(x => x.Value);
    }

    /// <inheritdoc cref="ICurrentUser.IsInRole" />
    public bool IsInRole(string role)
    {
        return httpContext.User.IsInRole(role);
    }
}
