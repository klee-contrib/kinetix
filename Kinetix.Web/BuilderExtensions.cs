using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.Web
{
    public static class BuilderExtensions
    {
        public static void AddWeb<TDbContext>(this MvcOptions builder)
            where TDbContext : DbContext
        {
            builder.Filters.AddService<CultureFilter>();
            builder.Filters.AddService<ExceptionFilter>();
            builder.Filters.AddService<TransactionFilter<TDbContext>>();
        }
    }
}
