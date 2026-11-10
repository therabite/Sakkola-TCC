using System.ComponentModel.DataAnnotations;

namespace Sakkola.Models.ViewModel
{
    public class RegisterColaborador
    {
        public int Id_colaborador { get; set; }
        public string nome { get; set; }
        public DateTime data_nasc { get; set; }
        public int telefone { get; set; }
        public int cpf { get; set; }
        public string email { get; set; }
        public string senha { get; set; }
        public string confirmarsenha { get; set; }
        public bool IsAdmin { get; set; }
    }
}
