using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sakkola.Models
{
    [Table("tbAddress")]
    public class Endereco
    {
        [Key]
        public int id_address { get; set; }

        [Required]
        public int cep { get; set; }
        [Required]
        public string logradouro { get; set; }
        [Required]
        public int numero { get; set; }
        [Required]
        public string bairro { get; set; }
        [Required]
        public string estado { get; set; }
    }
}
