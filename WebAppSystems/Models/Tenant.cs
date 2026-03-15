using System.ComponentModel.DataAnnotations;

namespace WebAppSystems.Models
{
    /// <summary>
    /// Representa um inquilino (cliente) no sistema SaaS
    /// Cada escritório/empresa terá seu próprio Tenant
    /// </summary>
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Nome do escritório/empresa

        [Required]
        [StringLength(50)]
        public string Subdomain { get; set; } // ex: "escritorio1" para escritorio1.seuapp.com

        [StringLength(20)]
        public string? Document { get; set; } // CNPJ/CPF com formatação

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? TrialEndsAt { get; set; }          // fim do período de trial

        public DateTime? SubscriptionExpiresAt { get; set; } // fim da assinatura paga

        public bool IsTrialExpired => TrialEndsAt.HasValue && DateTime.UtcNow > TrialEndsAt.Value && SubscriptionExpiresAt == null;
        public bool IsSubscriptionExpired => SubscriptionExpiresAt.HasValue && DateTime.UtcNow > SubscriptionExpiresAt.Value;
        public bool IsBlocked => !IsActive || IsTrialExpired || IsSubscriptionExpired;

        // Limites do plano
        public int MaxUsers { get; set; } = 5;
        public int MaxClients { get; set; } = 50;
        public long MaxStorageMB { get; set; } = 1024; // 1GB padrão

        // Relacionamentos
        public ICollection<Attorney> Attorneys { get; set; } = new List<Attorney>();
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<ProcessRecord> ProcessRecords { get; set; } = new List<ProcessRecord>();
    }
}
