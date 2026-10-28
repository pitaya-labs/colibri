using Microsoft.AspNetCore.Authorization;

namespace colibri_api.Common.Auth;

public interface ISlicePolicies
{
    static abstract void Configure(AuthorizationOptions options);
}

internal static class SlicePolicyDiscovery
{
    public static void ConfigureSlicePolicies(this AuthorizationOptions options)
    {
        var policyTypes = typeof(ISlicePolicies).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(ISlicePolicies)));

        foreach (var type in policyTypes)
            type.GetMethod(nameof(ISlicePolicies.Configure))!.Invoke(null, [options]);
    }
}
