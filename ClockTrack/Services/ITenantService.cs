namespace ClockTrack.Services
{
    /// <summary>
    /// Interface para gerenciar o contexto do Tenant atual
    /// </summary>
    public interface ITenantService
    {
        int GetTenantId();
        string GetTenantSubdomain();
        void SetTenant(int tenantId);
    }
}
