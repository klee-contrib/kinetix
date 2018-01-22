using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;

namespace Kinetix.Security
{
    /// <summary>
    /// Publie l'accès aux informations standard de l'utilisateur courant via les claims.
    /// </summary>
    public class StandardUser
    {
        /// <summary>
        /// Default public user id.
        /// </summary>
        public const int PublicUserId = -1;

        /// <summary>
        /// Droit par défaut pour les pages sans restriction.
        /// </summary>
        public const string PublicRole = "PUBLIC";

        private readonly IPrincipal _principal;

        public StandardUser(IPrincipal principal)
        {
            _principal = principal;
        }

        /// <summary>
        /// Retourne le nom de l'utilisateur.
        /// </summary>
        public string UserName => GetString(StandardClaims.UserName);

        /// <summary>
        /// Retourne la culture de l'utilisateur.
        /// </summary>
        public string Culture => GetString(StandardClaims.Culture);

        /// <summary>
        /// Retourne l'identité de l'utilisateur.
        /// </summary>
        public int? UserId => GetInt(StandardClaims.UserId);

        /// <summary>
        /// Retourne le login de l'utilisateur.
        /// </summary>
        public string Login => _principal.Identity.Name;

        /// <summary>
        /// Retourne le profil de l'utilisateur.
        /// </summary>
        public string ProfileId => GetString(StandardClaims.ProfileId);

        /// <summary>
        /// Indique si l'utilisateur courant est un super utilisateur.
        /// </summary>
        public bool IsSuperUser => GetString(StandardClaims.IsSuperUser) == "true";

        /// <summary>
        /// Indique si l'utilisateur courant est un utilisateur normal.
        /// </summary>
        public bool IsRegularUser => !IsSuperUser;

        /// <summary>
        /// Obtient la liste des rôles de l'utilisateur courant.
        /// </summary>
        /// <returns>Liste des droits de l'utilisateur courant.</returns>
        public ICollection<string> Roles
        {
            get
            {
                if (_principal.Identity is ClaimsIdentity identity)
                {
                    return identity
                        .FindAll(identity.RoleClaimType)
                        .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                        .Select(c => c.Value)
                        .ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Indique si l'utilisateur possède un rôle donné.
        /// </summary>
        /// <param name="role">Code du rôle.</param>
        /// <returns><code>True</code> si l'utilisateur possède le rôle.</returns>
        public bool IsInRole(string role)
        {
            return _principal.IsInRole(role);
        }

        /// <summary>
        /// Indique si l'utilisateur possède un des rôles passés en paramètre.
        /// </summary>
        /// <param name="roles">Les rôles à vérifier.</param>
        /// <returns><code>True</code> si l'utilisateur possède un des rôles.</returns>
        public bool IsInRoles(params string[] roles)
        {
            return roles.Any(IsInRole);
        }

        /// <summary>
        /// Vérifie que l'utilisateur courant possède un rôle donné.
        /// </summary>
        /// <param name="roles">Les roles d'accès au service.</param>
        public void CheckRole(params string[] roles)
        {
            bool authorized = roles.Any(IsInRole);
            if (!authorized)
            {
                throw new SecurityException("Un des rôles suivants est nécessaire : " + string.Concat(roles));
            }
        }

        /// <summary>
        /// Returns the data corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Data.</returns>
        public string GetString(string claimType)
        {
            if (_principal.Identity is ClaimsIdentity identity)
            {
                return identity.FindFirst(claimType)?.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the datas corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Liste des valeurs.</returns>
        protected ICollection<string> GetStrings(string claimType)
        {
            if (_principal.Identity is ClaimsIdentity identity)
            {
                return identity
                    .FindAll(claimType)
                    .Select(x => x.Value)
                    .ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the data corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Data.</returns>
        protected int? GetInt(string claimType)
        {
            var raw = GetString(claimType);
            if (raw == null)
            {
                return null;
            }

            return int.Parse(raw, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the data corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Data.</returns>
        protected bool GetBool(string claimType)
        {
            return GetString(claimType) == "true";
        }
    }
}