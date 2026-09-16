using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sakkola.Models
{
    [Table("tbUser")]
    public class Usuario
    {
        [Key]
        public int id_user { get; set; }

        [Required]
        [StringLength(50)]
        public string nome { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required, StringLength(8)]
        [DataType(DataType.Password)]
        public string senha { get; set; } = string.Empty;

        [Required]
        [Compare("senha")]
        public string confirmacaoSenha {  get; set; } = string.Empty;

        [Required]
        public DataType data_nasc {  get; set; }

        [Required]
        public string cpf {  get; set; } = string.Empty;

        [Required]
        public string telefone { get; set; } = string.Empty;

        [Required]
        public string rg { get; set; } = string.Empty;

    }
}
