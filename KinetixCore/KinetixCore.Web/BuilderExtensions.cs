using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web
{
    public static class BuilderExtensions
    {
        public static void AddWeb(this MvcOptions builder)
        {
            builder.Filters.AddService<CultureFilter>();
            builder.Filters.AddService<ExceptionFilter>();
        }

        public static void UseWeb(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<TransactionMiddleware>();
        }
    }
}
