namespace colibri_api.Common.Endpoints;

public interface IFeatureEndpoints
{
    static abstract void Map(IEndpointRouteBuilder app);
}

public static class EndpointDiscovery
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = typeof(EndpointDiscovery).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IFeatureEndpoints)));

        foreach (var type in endpointTypes)
            type.GetMethod(nameof(IFeatureEndpoints.Map))!.Invoke(null, [app]);

        return app;
    }
}
