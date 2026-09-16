using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sakkola.Models
{
    [Table("tbClient")]
    public class Client : Usuario
    {
        public int id_client { get; set; }
        public string nome { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string senha { get; set; } = string.Empty;
        public string confirmacaoSenha { get; set; } = string.Empty;
        public DateOnly data_nasc { get; set; }
        public string cpf { get; set; } = string.Empty;
        public string telefone { get; set; } = string.Empty;
        public string rg { get; set; } = string.Empty;
    }
}
