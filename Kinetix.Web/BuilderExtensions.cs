using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web
{
    public static class BuilderExtensions
    {
        public static void AddWeb(this MvcOptions builder)
        {
            builder.Filters.AddService<CultureFilter>();
            builder.Filters.AddService<ExceptionFilter>();
            builder.Filters.AddService<TransactionFilter>();
            builder.Filters.AddService<ReferenceCheckerFilter>();
            builder.Filters.AddService<UtcDateFilter>();
        }

        public static void ConfigureSerializer(this JsonOptions options)
        {
            options.JsonSerializerOptions.IgnoreNullValues = true;
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
        }
    }
}
