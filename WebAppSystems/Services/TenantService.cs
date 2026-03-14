using Microsoft.EntityFrameworkCore;
using WebAppSystems.Data;

namespace WebAppSystems.Services
{
    /// <summary>
    /// Serviço para identificar e gerenciar o Tenant atual da requisição
    /// </summary>
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _currentTenantId;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetTenantId()
        {
            if (_currentTenantId.HasValue)
                return _currentTenantId.Value;

            // Tenta obter do contexto HTTP (sessão ou claim)
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Primeiro tenta da sessão
                var tenantIdFromSession = httpContext.Session.GetInt32("TenantId");
                if (tenantIdFromSession.HasValue)
                {
                    _currentTenantId = tenantIdFromSession.Value;
                    return _currentTenantId.Value;
                }

                // Depois tenta do claim (JWT)
                var tenantIdClaim = httpContext.User?.FindFirst("TenantId")?.Value;
                if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out int tenantId))
                {
                    _currentTenantId = tenantId;
                    return _currentTenantId.Value;
                }
            }

            // Durante desenvolvimento/migrations, retorna tenant padrão
            // Em produção, isso deve lançar exceção
            return 1; // Tenant padrão
        }

        public string GetTenantSubdomain()
        {
            return GetSubdomainFromRequest() ?? string.Empty;
        }

        public void SetTenant(int tenantId)
        {
            _currentTenantId = tenantId;
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetInt32("TenantId", tenantId);
            }
        }

        private string? GetSubdomainFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            var host = httpContext.Request.Host.Host;
            
            // Ignora localhost e IPs
            if (host == "localhost" || host.StartsWith("127.") || host.StartsWith("192.168."))
                return null;

            var parts = host.Split('.');
            
            // Se tiver mais de 2 partes (ex: escritorio1.seuapp.com), o primeiro é o subdomínio
            if (parts.Length > 2)
                return parts[0];

            return null;
        }
    }
}
