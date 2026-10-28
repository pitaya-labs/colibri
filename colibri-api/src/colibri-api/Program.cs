using FluentValidation;
using colibri_api.Common.Auth;
using colibri_api.Common.Cors;
using colibri_api.Common.Endpoints;
using colibri_api.Common.ErrorHandling;
using colibri_api.Common.Events;
using colibri_api.Common.HealthChecks;
using colibri_api.Common.Logging;
using colibri_api.Common.OpenApi;
using colibri_api.Common.Persistence;
using colibri_api.Common.RateLimiting;
using colibri_api.Composition;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();


builder.Services
    .AddGlobalErrorHandling(builder.Environment)
    .AddAppCors(builder.Environment, builder.Configuration)
    .AddAppHealthChecks()
    .AddAppRateLimiting(builder.Configuration)
    .AddOpenApiDocument()
    .AddAppDbContext(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddSingleton(TimeProvider.System)
    .AddEvents()
    .AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddUseCaseHandlers();

var app = builder.Build();

app.UseCorrelationId();
app.UseAppRequestLogging();
app.UseGlobalErrorHandling();
app.ApplyPendingMigrations();
app.MapAppHealthChecks();
app.MapOpenApiDocument();
app.UseCors();
app.UseAuthentication();
app.UseAppRateLimiting();
app.UseAuthorization();
app.MapFeatureEndpoints();

app.Run();

public partial class Program;
