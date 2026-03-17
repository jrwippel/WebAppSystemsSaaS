using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ClockTrack.Data;
using ClockTrack.Models;

namespace ClockTrack.Filters
{
    public class VerificarAssinatura : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string sessaoUsuario = context.HttpContext.Session.GetString("sessaoUsuarioLogado");
            if (string.IsNullOrEmpty(sessaoUsuario))
            {
                base.OnActionExecuting(context);
                return;
            }

            var attorney = JsonConvert.DeserializeObject<Attorney>(sessaoUsuario);
            if (attorney == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            var db = context.HttpContext.RequestServices.GetService<ClockTrackContext>();
            if (db == null)
            {
                base.OnActionExecuting(context);
                return;
            }

            var tenant = db.Tenants.Find(attorney.TenantId);
            if (tenant != null && tenant.IsBlocked)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Assinatura" },
                    { "action", "Expirado" }
                });
            }

            base.OnActionExecuting(context);
        }
    }
}
