namespace colibri_api.Composition;

public static class UseCaseRegistration
{
    public static IServiceCollection AddUseCaseHandlers(this IServiceCollection services) =>
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.Where(t => t is { IsPublic: true, IsAbstract: false } && t.Name.EndsWith("Handler")))
            .AsSelf()
            .WithScopedLifetime());
}
