using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Security
{
    /// <summary>
    /// Implémentation de ICurrentUser via HttpContext.
    /// </summary>
    public class WebUser : ICurrentUser
    {
        private readonly HttpContext _httpContext;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="httpContext">HttpContext.</param>
        public WebUser(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        /// <inheritdoc cref="ICurrentUser.Login" />
        public string Login => _httpContext.User.Identity.Name;

        /// <inheritdoc cref="ICurrentUser.Roles" />
        public IEnumerable<string> Roles =>
            _httpContext.User.Identity is not ClaimsIdentity identity
                ? null
                : identity
                    .FindAll(identity.RoleClaimType)
                    .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                    .Select(c => c.Value);


        /// <inheritdoc cref="ICurrentUser.GetString" />
        public string GetString(string claimType)
        {
            if (_httpContext.User.Identity is not ClaimsIdentity identity)
            {
                return null;
            }

            var claim = identity.FindFirst(claimType);
            return claim?.Value;
        }

        /// <inheritdoc cref="ICurrentUser.GetStrings" />
        public IEnumerable<string> GetStrings(string claimType)
        {
            return _httpContext.User.Identity is not ClaimsIdentity identity
                ? null
                : identity.FindAll(claimType).Select(x => x.Value);
        }

        /// <inheritdoc cref="ICurrentUser.IsInRole" />
        public bool IsInRole(string role)
        {
            return _httpContext.User.IsInRole(role);
        }
    }
}