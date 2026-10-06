using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sakkola.Models
{
    [Table("tbAddress")]
    public class Endereco
    {
        [Key]
        public int Id_address { get; set; }
        // Em RegisterClient.cs, se quiser manter a validação, garanta que o nome seja 'Cep' (com C maiúsculo):
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Informe um CEP válido.")]
        public new string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public int Numero { get; set; } 
        public string Bairro { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
