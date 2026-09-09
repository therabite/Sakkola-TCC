using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models
{
    public class Client : Usuario
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string senha { get; set; } = string.Empty;
        public string confirmacaoSenha { get; set; } = string.Empty;
        public DataType dataNasc { get; set; }
        public string cpf { get; set; } = string.Empty;
        public string telefone { get; set; } = string.Empty;
        public string rg { get; set; } = string.Empty;

    }
}
