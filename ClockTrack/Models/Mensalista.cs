using ClockTrack.Services;

namespace ClockTrack.Models
{
    public class Mensalista : ITenantEntity
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public int ClientId { get; set; }
        public decimal ValorMensalBruto { get; set; }
        public decimal ComissaoParceiro { get; set; }
        public decimal ComissaoSocio { get; set; }

        // Relação com a tabela associativa
        //public ICollection<MensalistaDepartment> MensalistaDepartments { get; set; } = new List<MensalistaDepartment>();
        
        // Multi-tenant
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public Mensalista()
        {
        }

        public Mensalista(int id, Client client, int clientId, decimal valorMensalBruto, decimal comissaoParceiro, decimal comissaoSocio)
        {
            Id = id;
            Client = client;
            ClientId = clientId;
            ValorMensalBruto = valorMensalBruto;
            ComissaoParceiro = comissaoParceiro;
            ComissaoSocio = comissaoSocio;
        }

        public static implicit operator Mensalista(MensalistaService v)
        {
            throw new NotImplementedException();
        }
    }
}

