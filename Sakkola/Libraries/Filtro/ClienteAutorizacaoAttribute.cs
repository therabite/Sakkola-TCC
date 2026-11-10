using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sakkola.Libraries.Filtro
{
    public class ClienteAutorizacaoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (httpContext.Session.GetInt32("IdCliente") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Cliente", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
