using System.ComponentModel.DataAnnotations;

namespace ClockTrack.Models
{
    public class RegistroTenantModel
    {
        // Dados da Empresa (Tenant)
        [Required(ErrorMessage = "Nome da empresa é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome da empresa deve ter no máximo 100 caracteres")]
        public string NomeEmpresa { get; set; }

        [Required(ErrorMessage = "Subdomínio é obrigatório")]
        [StringLength(50, ErrorMessage = "Subdomínio deve ter no máximo 50 caracteres")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomínio deve conter apenas letras minúsculas, números e hífens")]
        public string Subdomain { get; set; }

        [StringLength(20, ErrorMessage = "CNPJ/CPF deve ter no máximo 20 caracteres")]
        public string? Document { get; set; }

        [Required(ErrorMessage = "Email da empresa é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string EmailEmpresa { get; set; }

        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string? TelefoneEmpresa { get; set; }

        // Dados do Usuário Administrador
        [Required(ErrorMessage = "Nome do administrador é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string NomeAdmin { get; set; }

        [Required(ErrorMessage = "Email do administrador é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string EmailAdmin { get; set; }

        [Required(ErrorMessage = "Login é obrigatório")]
        [StringLength(50, ErrorMessage = "Login deve ter no máximo 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Login deve conter apenas letras, números, ponto, hífen e underscore")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d).{8,}$", 
            ErrorMessage = "Senha deve conter pelo menos 8 caracteres, incluindo letras e números")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("Senha", ErrorMessage = "Senha e confirmação não conferem")]
        public string ConfirmarSenha { get; set; }

        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string? TelefoneAdmin { get; set; }
    }
}
