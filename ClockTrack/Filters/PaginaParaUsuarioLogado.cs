using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Filters
{
    public class PaginaParaUsuarioLogado : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
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
                return;
            }

            var attorney = JsonConvert.DeserializeObject<Attorney>(sessaoUsuario);
            if (attorney == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Login" }, { "action", "Index" } });
                return;
            }

            // Verifica se a assinatura/trial do tenant está ativa
            var db = context.HttpContext.RequestServices.GetService<ClockTrackContext>();
            if (db != null)
            {
                var tenant = db.Tenants.Find(attorney.TenantId);
                if (tenant != null && tenant.IsBlocked)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Assinatura" },
                        { "action", "Expirado" }
                    });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
    }
}
