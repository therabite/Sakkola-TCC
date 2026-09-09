using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string name { get; set; } = string.Empty;
        
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
        [DataType(DataType.Date)]
        public DataType dataNasc {  get; set; }

        [Required]
        public string cpf {  get; set; } = string.Empty;

        [Required]
        public string telefone { get; set; } = string.Empty;

        [Required]
        public string rg { get; set; } = string.Empty;

    }
}
