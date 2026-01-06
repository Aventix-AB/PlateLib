using FluentValidation;

namespace API.Filters;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
	private readonly IValidator<T> _validator = validator;

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		var request = context.Arguments.OfType<T>().FirstOrDefault();

		if (request == null)
		{
			return Results.BadRequest("No request for validation");
		}
		var result = await _validator.ValidateAsync(request);

		return result.IsValid
			? await next(context)
			: Results.ValidationProblem(result.ToDictionary());

	}
}
