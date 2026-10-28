using FluentValidation;

namespace colibri_api.Common.Endpoints;

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class =>
        builder
            .AddEndpointFilter(async (context, next) =>
            {
                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<IValidator<TRequest>>();

                var request = context.Arguments.OfType<TRequest>().First();
                var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

                return result.IsValid
                    ? await next(context)
                    : TypedResults.ValidationProblem(result.ToDictionary());
            })
            .ProducesValidationProblem();
}
