using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models.ViewModel
{
    public class RegisterColaborador
    {
        public string Nome { get; set; }
        public DateTime DataNasc { get; set; }
        public string Telefone { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarSenha { get; set; }
        public bool IsAdmin { get; set; }
    }
}
