using System.ComponentModel.DataAnnotations;

namespace WebAppSystems.Models
{
    public class ValorCliente : ITenantEntity
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public int ClientId { get; set; }
        public Attorney Attorney { get; set; }
        public int AttorneyId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public double Valor { get; set; }
        
        // Multi-tenant
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
