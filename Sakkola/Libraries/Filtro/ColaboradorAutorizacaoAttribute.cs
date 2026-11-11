using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sakkola.Libraries.Filtro
{
    public class ColaboradorAutorizacaoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessaoEmail = context.HttpContext.Session.GetString("EmailColaborador");

            if (string.IsNullOrEmpty(sessaoEmail))
            {
                context.Result = new RedirectToActionResult("Login", "Colaborador", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
