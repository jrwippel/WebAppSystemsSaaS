using System.ComponentModel.DataAnnotations;

namespace ClockTrack.Models
{
    public class PercentualArea : ITenantEntity
    {
        public int Id { get; set; }
        public Department Department { get; set; }
        public int DepartmentId { get; set; }
        public Client Client { get; set; }
        public int ClientId { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Percentual { get; set; }
        
        // Multi-tenant
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
