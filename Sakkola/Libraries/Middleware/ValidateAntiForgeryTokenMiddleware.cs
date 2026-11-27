using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Antiforgery;

namespace Sakkola.Libraries.Middleware
{
    public class ValidateAntiForgeryTokenAttribute : TypeFilterAttribute
    {
        public ValidateAntiForgeryTokenAttribute() : base(typeof(ValidateAntiForgeryTokenFilter))
        {
        }

        private class ValidateAntiForgeryTokenFilter : IAsyncActionFilter
        {
            private readonly IAntiforgery _antiforgery;

            public ValidateAntiForgeryTokenFilter(IAntiforgery antiforgery)
            {
                _antiforgery = antiforgery;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var httpContext = context.HttpContext;
                if (HttpMethods.IsPost(httpContext.Request.Method) ||
                    HttpMethods.IsPut(httpContext.Request.Method) ||
                    HttpMethods.IsDelete(httpContext.Request.Method))
                {
                    try
                    {
                        await _antiforgery.RequestTokenValidationAsync(httpContext);
                    }
                    catch (AntiforgeryValidationException)
                    {
                        context.Result = new BadRequestObjectResult("Token Anti-Forgery inválido ou ausente.");
                        return;
                    }
                }

                await next();
            }
        }
    }
}