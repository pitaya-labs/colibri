using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;
using colibri_api.Common.Endpoints;
using colibri_api.Common.Events;
using colibri_api.Composition;

namespace colibri_api.Tests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly Api = typeof(Program).Assembly;
    private const string FeaturesRoot = "colibri_api.Features";
    private const string CommonRoot = "colibri_api.Common";

    [Fact]
    public void A_slice_does_not_use_use_cases_from_another_slice()
    {
        var failures = new List<string>();

        foreach (var slice in Slices())
        {
            var foreignUseCases = Slices()
                .Where(other => other != slice)
                .SelectMany(UseCaseTypeNames)
                .ToArray();

            var result = Types.InAssembly(Api)
                .That().ResideInNamespaceStartingWith($"{FeaturesRoot}.{slice}")
                .ShouldNot().HaveDependencyOnAny(foreignUseCases)
                .GetResult();

            if (!result.IsSuccessful)
                failures.AddRange(result.FailingTypes.Select(t => $"  {t.FullName}"));
        }

        Assert.True(failures.Count == 0,
            "Estos tipos usan casos de uso de otro slice. Lo público de un slice es su " +
            "Domain (consultas) y sus Contracts (eventos); para lo demás, comunícate por " +
            $"eventos (README, regla 1):\n{string.Join('\n', failures)}");
    }

    [Fact]
    public void Common_does_not_use_the_slices_use_cases()
    {
        var allUseCases = Slices().SelectMany(UseCaseTypeNames).ToArray();

        var result = Types.InAssembly(Api)
            .That().ResideInNamespaceStartingWith(CommonRoot)
            .ShouldNot().HaveDependencyOnAny(allUseCases)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common es infraestructura compartida: puede conocer el Domain de los slices " +
            "(AppDbContext), pero nunca sus casos de uso. Violaciones:\n" +
            string.Join('\n', (result.FailingTypes ?? []).Select(t => $"  {t.FullName}")));
    }

    [Fact]
    public void Every_Endpoints_class_implements_IFeatureEndpoints()
    {
        var offenders = Api.GetTypes()
            .Where(t => t.Name.EndsWith("Endpoints") && IsInFeatures(t))
            .Where(t => !t.IsAssignableTo(typeof(IFeatureEndpoints)))
            .Select(t => $"  {t.FullName}")
            .ToList();

        Assert.True(offenders.Count == 0,
            "Estas clases parecen de endpoints pero no implementan IFeatureEndpoints, así " +
            "que el descubrimiento automático las ignora y sus rutas NO existen:\n" +
            string.Join('\n', offenders));
    }

    [Fact]
    public void Event_handlers_live_in_Features_not_in_Common()
    {
        var offenders = Api.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Where(t => !IsInFeatures(t))
            .Select(t => $"  {t.FullName}")
            .ToList();

        Assert.True(offenders.Count == 0,
            "Reaccionar a un evento es lógica de negocio y pertenece a un slice, no a " +
            "Common (README, regla del sentido de la flecha):\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void Nested_contracts_of_use_cases_are_named_Request_or_Response()
    {
        string[] allowed = ["Request", "Response", "Validator"];

        var offenders = Api.GetTypes()
            .Where(t => t.IsNestedPublic && IsInFeatures(t))
            .Where(t => !allowed.Contains(t.Name))
            .Select(t => $"  {t.FullName}")
            .ToList();

        Assert.True(offenders.Count == 0,
            "Los tipos públicos anidados en un caso de uso son su contrato y se llaman " +
            "exactamente Request o Response (también definen los nombres del cliente " +
            "TypeScript generado). Si el tipo no es un contrato, muévelo fuera de la clase " +
            "del caso de uso o amplía la lista de este test:\n" + string.Join('\n', offenders));
    }

    [Fact]
    public void Todo_handler_de_caso_de_uso_queda_registrado_por_AddUseCaseHandlers()
    {
        var handlerTypes = Api.GetTypes()
            .Where(t => t is { IsPublic: true, IsAbstract: false } && IsInFeatures(t) && t.Name.EndsWith("Handler"))
            .ToArray();

        var services = new ServiceCollection();
        services.AddUseCaseHandlers();

        var missing = handlerTypes
            .Where(t => services.All(d => d.ServiceType != t))
            .Select(t => $"  {t.FullName}")
            .ToList();

        Assert.True(missing.Count == 0,
            "Estos handlers no quedaron registrados por AddUseCaseHandlers() — revisa el filtro de " +
            "Scrutor en Composition/UseCaseRegistration.cs (¿la clase es pública? ¿termina en " +
            $"\"Handler\"?):\n" + string.Join('\n', missing));
    }

    private static bool IsInFeatures(Type t) =>
        t.Namespace?.StartsWith($"{FeaturesRoot}.") == true;

    private static string[] Slices() => Api.GetTypes()
        .Where(IsInFeatures)
        .Select(t => t.Namespace![(FeaturesRoot.Length + 1)..].Split('.')[0])
        .Distinct()
        .ToArray();

    private static string[] UseCaseTypeNames(string slice) => Api.GetTypes()
        .Where(t => !t.IsNested && t.Namespace == $"{FeaturesRoot}.{slice}")
        .Select(t => t.FullName!)
        .ToArray();
}
