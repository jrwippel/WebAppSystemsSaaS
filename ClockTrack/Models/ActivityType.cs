using System.ComponentModel.DataAnnotations;

namespace ClockTrack.Models
{
    /// <summary>
    /// Tipos de atividade personalizáveis por tenant
    /// Substitui o enum RecordType fixo para permitir customização
    /// </summary>
    public class ActivityType : ITenantEntity
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome do tipo de atividade é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Tipo")]
        public string Name { get; set; }
        
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
        
        [Display(Name = "Cor (Hex)")]
        [StringLength(7, ErrorMessage = "Cor deve estar no formato #RRGGBB")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato #RRGGBB (ex: #FF5733)")]
        public string Color { get; set; } // Para exibição em gráficos/relatórios
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Ordem de Exibição")]
        public int DisplayOrder { get; set; }
        
        // Multi-tenant
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        
        // Relacionamento com ProcessRecords
        public ICollection<ProcessRecord> ProcessRecords { get; set; }
    }
}
