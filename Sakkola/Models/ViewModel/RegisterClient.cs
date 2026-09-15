using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models.ViewModel
{
    public class RegisterClient : Endereco
    {
        [Required(ErrorMessage = "O nome é obrigatório para cadastro.")]
        [StringLength(50, ErrorMessage = "O nome não pode exceder 50 caracteres.")]
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("senha", ErrorMessage = "As senhas não conferem.")]
        public string confirmarSenha { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "A data de nascimentos é obrigatório para cadastro.")]
        public DateOnly dataNasc { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        ///
        ///
        ///
        [Required(ErrorMessage = "O número de telefone é obrigatório para cadastro.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "informe um telefone válido")]
        public string telefone { get; set; } = string.Empty;
        ///
        ///
        ///
        [Required(ErrorMessage = "O CPF é obrigatório para cadastro.")]
        [RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", ErrorMessage = "Informe um CPF válido.")]
        public string cpf { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "o RG é obrigatório para cadastro.")]
        [RegularExpression(@"^\d{2}\.?\d{3}\.?\d{3}-?[0-9Xx]$", ErrorMessage = "Informe um RG válido.")]
        public string rg { get; set; } = string.Empty;

        public string cep { get; set; }
    }
}
