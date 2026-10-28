using Serilog;
using Serilog.Formatting.Json;

namespace colibri_api.Common.Logging;

public static class LoggingRegistration
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            if (context.HostingEnvironment.IsDevelopment())
                configuration.WriteTo.Console();
            else
                configuration.WriteTo.Console(new JsonFormatter());
        }, preserveStaticLogger: true);

        return builder;
    }

    public static WebApplication UseAppRequestLogging(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<Serilog.ILogger>();

        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/health"),
            branch => branch.UseSerilogRequestLogging(options => options.Logger = logger));

        return app;
    }
}
