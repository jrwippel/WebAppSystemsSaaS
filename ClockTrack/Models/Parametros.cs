using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClockTrack.Models
{
    public class Parametros : ITenantEntity
    {
        [Key] // Adicionando a chave primária
        public int Id { get; set; }

        [NotMapped]
        public IFormFile Logo { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A largura deve ser maior que 0")]
        public int Width { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A altura deve ser maior que 0")]
        public int Height { get; set; }

        // Propriedades para armazenar a imagem no banco de dados
        public byte[] LogoData { get; set; }
        public string LogoMimeType { get; set; }
        
        // Multi-tenant
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }

        [Range(0, 100, ErrorMessage = "A alíquota deve ser entre 0 e 100")]
        public decimal AliquotaTributos { get; set; } = 14.53m;
    }
}
