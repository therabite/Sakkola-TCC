using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models.ViewModel
{
    public class LoginClient
    {
        [Required(ErrorMessage = "O email é obrigatório para cadastro.")]
        [StringLength(50, ErrorMessage = "O email não pode exceder 50 caracteres.")]
        [EmailAddress(ErrorMessage = "Informe um email válido.")]
        public string email { get; set; } = string.Empty;
        /// 
        ///
        ///
        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string senha { get; set; } = string.Empty;
        ///
        ///
        ///
        [Display(Name = "Lembrar-me")]
        public bool lembraDeMim {  get; set; }
    }
}
