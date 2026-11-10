using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sakkola.Libraries.Filtro
{
    public class ColaboradorAutorizacaoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (httpContext.Session.GetInt32("IdColaborador") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Colaborador", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
