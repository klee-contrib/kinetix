using System.Text.Json.Serialization;
using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Kinetix.Web;

public static class BuilderExtensions
{
    public static MvcOptions AddWeb(this MvcOptions builder)
    {
        builder.Filters.AddService<CultureFilter>();
        builder.Filters.AddService<ExceptionFilter>();
        builder.Filters.AddService<TransactionFilter>();
        builder.Filters.AddService<ReferenceCheckerFilter>();
        builder.Filters.AddService<UtcDateFilter>();
        
        return builder;
    }

    public static void ConfigureSerializer(this JsonOptions options)
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
    }
}
