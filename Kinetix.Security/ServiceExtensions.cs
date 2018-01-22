using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Security
{
    /// <summary>
    /// Méthodes d'extensions.
    /// </summary>
    public static class ServiceExtensions
    {
        public static void AddSecurity<TUser>(this IServiceCollection services)
            where TUser : StandardUser
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddTransient<StandardUser, TUser>();
            services.AddTransient<TUser>();
        }

        /// <summary>
        /// Indique si l'identité est autorisé à avoir accès à l'application.
        /// </summary>
        /// <param name="identity">Identité.</param>
        /// <returns><code>True</code> si l'accès est autorisé.</returns>
        public static bool IsAuthorized(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (identity is ClaimsIdentity claimsIdentity)
            {
                return claimsIdentity
                    .FindAll(StandardClaims.IsAuthorized)
                    .Where(c => c.Issuer == ClaimsIdentity.DefaultIssuer)
                    .Any(c => c.Value == "true");
            }
            else
            {
                return false;
            }
        }
    }
}
