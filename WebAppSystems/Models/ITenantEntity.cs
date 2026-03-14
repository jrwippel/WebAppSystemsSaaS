namespace WebAppSystems.Models
{
    /// <summary>
    /// Interface para entidades que pertencem a um Tenant
    /// Todas as entidades principais devem implementar esta interface
    /// </summary>
    public interface ITenantEntity
    {
        int TenantId { get; set; }
        Tenant? Tenant { get; set; }
    }
}
