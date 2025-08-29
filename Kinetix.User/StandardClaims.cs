using System.Security.Claims;

namespace Kinetix.User;

/// <summary>
/// Nom des claims de sécurité standard.
/// </summary>
public static class StandardClaims
{
    /// <summary>
    /// User name claim.
    /// </summary>
    public const string Culture = "Culture";

    /// <summary>
    /// Authorized flag claim.
    /// </summary>
    public const string IsAuthorized = "IsAuthorized";

    /// <summary>
    /// Super user flag claim.
    /// </summary>
    public const string IsSuperUser = "IsSuperUser";

    /// <summary>
    /// Nom du claim.
    /// </summary>
    public const string ProfileId = "Profile";

    /// <summary>
    /// Authorized flag claim.
    /// </summary>
    public const string Role = ClaimTypes.Role;

    /// <summary>
    /// ID de l'utilisateur.
    /// </summary>
    public const string UserId = "UserId";

    /// <summary>
    /// User name claim.
    /// </summary>
    public const string UserName = "UserName";
}
