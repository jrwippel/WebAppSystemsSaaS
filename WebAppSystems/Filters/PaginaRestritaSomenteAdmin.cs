using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using WebAppSystems.Models;
using WebAppSystems.Models.Enums;

namespace WebAppSystems.Filters
{
    public class PaginaRestritaSomenteAdmin : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string sessaoUsuario = context.HttpContext.Session.GetString("sessaoUsuarioLogado");

            if (string.IsNullOrEmpty(sessaoUsuario))
            {
                var tempData = context.HttpContext.RequestServices
                    .GetService<ITempDataDictionaryFactory>()
                    ?.GetTempData(context.HttpContext);
                if (tempData != null)
                    tempData["MensagemAviso"] = "A sessão expirou. Por favor, faça login novamente.";

                context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } });
            }
            else
            {
                Attorney attorney = JsonConvert.DeserializeObject<Attorney>(sessaoUsuario);
                if (attorney == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } });
                }

                if (attorney.Perfil == ProfileEnum.Padrao)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Restrito" }, { "action", "Index" } });
                }
            }
            base.OnActionExecuted(context);
        }
    }
}
