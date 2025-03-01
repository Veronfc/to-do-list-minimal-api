public class AuthFilter : IEndpointFilter
{
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    var user = context.HttpContext.User;

    if (!user.Identity?.IsAuthenticated ?? true)
    {
      return Results.Unauthorized();
    }

    return await next(context);
  }
}