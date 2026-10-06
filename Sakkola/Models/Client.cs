using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sakkola.Models
{
    [Table("tbClient")]
    public class Client : Usuario
    {
        public int Id_client { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string ConfirmacaoSenha { get; set; } = string.Empty;
        public DateOnly DataNasc { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Rg { get; set; } = string.Empty;
    }
}
