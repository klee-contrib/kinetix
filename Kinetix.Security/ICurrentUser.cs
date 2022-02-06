using System.Globalization;
using System.Security;

namespace Kinetix.Security;

/// <summary>
/// Publie l'accès aux informations de l'utilisateur courant.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Retourne le login de l'utilisateur.
    /// </summary>
    string Login { get; }

    /// <summary>
    /// Obtient la liste des rôles de l'utilisateur courant.
    /// </summary>
    /// <returns>Liste des droits de l'utilisateur courant.</returns>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Retourne le nom de l'utilisateur.
    /// </summary>
    string UserName => GetString(StandardClaims.UserName);

    /// <summary>
    /// Retourne la culture de l'utilisateur.
    /// </summary>
    string Culture => GetString(StandardClaims.Culture);

    /// <summary>
    /// Retourne l'identité de l'utilisateur.
    /// </summary>
    int? UserId => GetInt(StandardClaims.UserId);

    /// <summary>
    /// Retourne le profil de l'utilisateur.
    /// </summary>
    string ProfileId => GetString(StandardClaims.ProfileId);

    /// <summary>
    /// Indique si l'utilisateur courant est un super utilisateur.
    /// </summary>
    bool IsSuperUser => GetString(StandardClaims.IsSuperUser) == "true";

    /// <summary>
    /// Indique si l'utilisateur courant est un utilisateur normal.
    /// </summary>
    bool IsRegularUser => !IsSuperUser;

    /// <summary>
    /// Returns the data corresponding to the claim type.
    /// </summary>
    /// <param name="claimType">Claim Type.</param>
    /// <returns>Data.</returns>
    string GetString(string claimType);

    /// <summary>
    /// Returns the datas corresponding to the claim type.
    /// </summary>
    /// <param name="claimType">Claim Type.</param>
    /// <returns>Liste des valeurs.</returns>
    IEnumerable<string> GetStrings(string claimType);

    /// <summary>
    /// Indique si l'utilisateur possède un rôle donné.
    /// </summary>
    /// <param name="role">Code du rôle.</param>
    /// <returns><code>True</code> si l'utilisateur possède le rôle.</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Vérifie que l'utilisateur courant possède un rôle donné.
    /// </summary>
    /// <param name="roles">Les roles d'accès au service.</param>
    void CheckRole(params string[] roles)
    {
        if (!IsInRoles(roles))
        {
            throw new SecurityException($"Un des rôles suivants est nécessaire : {string.Join(", ", roles)}");
        }
    }

    /// <summary>
    /// Returns the data corresponding to the claim type.
    /// </summary>
    /// <param name="claimType">Claim Type.</param>
    /// <returns>Data.</returns>
    bool GetBool(string claimType)
    {
        return GetString(claimType) == "true";
    }

    /// <summary>
    /// Returns the data corresponding to the claim type.
    /// </summary>
    /// <param name="claimType">Claim Type.</param>
    /// <returns>Data.</returns>
    int? GetInt(string claimType)
    {
        var raw = GetString(claimType);
        return raw == null
            ? null
            : int.Parse(raw, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Vérifie que l'utilisateur courant possède un rôle donné.
    /// </summary>
    /// <param name="roles">Les roles d'accès au service.</param>
    bool IsInRoles(params string[] roles)
    {
        return roles.Any(role => IsInRole(role));
    }
}
