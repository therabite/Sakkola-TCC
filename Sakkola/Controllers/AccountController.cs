using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewModel;

namespace Sakkola.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("conexaoMySQL");
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(RegisterClient registerClient, Endereco endereco)
        {
            // 1. Caminho de falha de validação do formulário
            if (!ModelState.IsValid)
            {
                return View(registerClient);
            }

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                INSERT INTO tbAddress (CEP) VALUES (@cep);
                INSERT INTO tbUser (nome, data_nasc, telefone, CPF, email, senha) 
                VALUES (@nome, @data_nasc, @telefone, @CPF, @email, @senha);
                INSERT INTO tbClient (id_client) VALUES (LAST_INSERT_ID());";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        string cepLimpo = System.Text.RegularExpressions.Regex.Replace(endereco.Cep ?? "", @"\D", "");
                        string cpfLimpo = System.Text.RegularExpressions.Regex.Replace(registerClient.cpf ?? "", @"\D", "");
                        string telefoneLimpo = System.Text.RegularExpressions.Regex.Replace(registerClient.telefone ?? "", @"\D", "");

                        cmd.Parameters.AddWithValue("@cep", cepLimpo);
                        cmd.Parameters.AddWithValue("@nome", registerClient.nome);
                        cmd.Parameters.AddWithValue("@data_nasc", registerClient.data_nasc);
                        cmd.Parameters.AddWithValue("@telefone", telefoneLimpo);
                        cmd.Parameters.AddWithValue("@CPF", cpfLimpo);
                        cmd.Parameters.AddWithValue("@email", registerClient.email);
                        cmd.Parameters.AddWithValue("@senha", registerClient.senha);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // 2. Caminho de sucesso
                TempData["Mensagem"] = "Cadastro realizado com sucesso!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao guardar os dados: {ex.Message}");
                return View(registerClient);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, bool remember)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Email e senha são obrigatórios.");
                return View();
            }
            bool usuarioValido = ValidarUsuarioNoBanco(email, password);
            if (!usuarioValido)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View();
            }
            TempData["Mensagem"] = "Login efetuado com sucesso!";

            return RedirectToAction("Index", "Home");
        }
        private bool ValidarUsuarioNoBanco(string email, string password)
        {
            using (var conexao = new MySqlConnection(_connectionString))
            {
                conexao.Open();

                var cmd = new MySqlCommand();
                
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@senha", password);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

    }
}