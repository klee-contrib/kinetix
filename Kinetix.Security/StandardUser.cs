﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading;

namespace Kinetix.Security
{
    /// <summary>
    /// Publie l'accès aux informations standard de l'utilisateur courant via les claims.
    /// </summary>
    public static class StandardUser
    {
        /// <summary>
        /// Default public user id.
        /// </summary>
        public const int PublicUserId = -1;

        /// <summary>
        /// Droit par défaut pour les pages sans restriction.
        /// </summary>
        public const string PublicRole = "PUBLIC";

        /// <summary>
        /// Retourne le nom de l'utilisateur.
        /// </summary>
        public static string UserName => GetString(StandardClaims.UserName);

        /// <summary>
        /// Retourne la culture de l'utilisateur.
        /// </summary>
        public static string Culture => GetString(StandardClaims.Culture);

        /// <summary>
        /// Retourne l'identité de l'utilisateur.
        /// </summary>
        public static int? UserId => GetInt(StandardClaims.UserId);

        /// <summary>
        /// Retourne le login de l'utilisateur.
        /// </summary>
        public static string Login => Thread.CurrentPrincipal?.Identity.Name;

        /// <summary>
        /// Retourne le profil de l'utilisateur.
        /// </summary>
        public static string ProfileId => GetString(StandardClaims.ProfileId);

        /// <summary>
        /// Indique si l'utilisateur courant est un super utilisateur.
        /// </summary>
        public static bool IsSuperUser => GetString(StandardClaims.IsSuperUser) == "true";

        /// <summary>
        /// Indique si l'utilisateur courant est un utilisateur normal.
        /// </summary>
        public static bool IsRegularUser => !IsSuperUser;

        /// <summary>
        /// Obtient la liste des rôles de l'utilisateur courant.
        /// </summary>
        /// <returns>Liste des droits de l'utilisateur courant.</returns>
        public static ICollection<string> Roles =>
            Thread.CurrentPrincipal?.Identity is not ClaimsIdentity identity
                ? null
                : identity
                    .FindAll(identity.RoleClaimType)
                    .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                    .Select(c => c.Value)
                    .ToList();

        /// <summary>
        /// Indique si l'utilisateur possède un rôle donné.
        /// </summary>
        /// <param name="role">Code du rôle.</param>
        /// <returns><code>True</code> si l'utilisateur possède le rôle.</returns>
        public static bool IsInRole(string role)
        {
            return Thread.CurrentPrincipal?.IsInRole(role) ?? false;
        }

        /// <summary>
        /// Indique si l'utilisateur possède un des rôles passés en paramètre.
        /// </summary>
        /// <param name="roles">Les rôles à vérifier.</param>
        /// <returns><code>True</code> si l'utilisateur possède un des rôles.</returns>
        public static bool IsInRoles(params string[] roles)
        {
            return roles.Any(role => IsInRole(role));
        }

        /// <summary>
        /// Vérifie que l'utilisateur courant possède un rôle donné.
        /// </summary>
        /// <param name="roles">Les roles d'accès au service.</param>
        public static void CheckRole(string[] roles)
        {
            var authorized = roles.Any(role => IsInRole(role));
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
        public static string GetString(string claimType)
        {
            if (Thread.CurrentPrincipal?.Identity is not ClaimsIdentity identity)
            {
                return null;
            }

            var claim = identity.FindFirst(claimType);
            return claim?.Value;
        }

        /// <summary>
        /// Returns the datas corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Liste des valeurs.</returns>
        public static ICollection<string> GetStrings(string claimType)
        {
            return Thread.CurrentPrincipal?.Identity is not ClaimsIdentity identity
                ? null
                : identity
                    .FindAll(claimType)
                    .Select(x => x.Value)
                    .ToList();
        }

        /// <summary>
        /// Returns the data corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Data.</returns>
        public static int? GetInt(string claimType)
        {
            var raw = GetString(claimType);
            return raw == null
                ? null
                : int.Parse(raw, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the data corresponding to the claim type.
        /// </summary>
        /// <param name="claimType">Claim Type.</param>
        /// <returns>Data.</returns>
        public static bool GetBool(string claimType)
        {
            return GetString(claimType) == "true";
        }
    }
}