using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlConnector;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
using System.Security.Claims;

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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginClient loginClient)
        {
            if (!ModelState.IsValid)
            {
                return View(loginClient);
            }
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    string sql = "SELECT id_User, email, senha from tbUser where email = @email and senha = @senha limit 1;";

                    using(var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", loginClient.Email);
                        cmd.Parameters.AddWithValue("@senha", loginClient.Senha);

                        using(var reader = await cmd.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                int idUser = reader.GetInt32("Id_user");
                                string email = reader.GetString("email");
                                string senha = reader.GetString("senha"); 
                            
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, idUser.ToString()),
                                    new Claim(ClaimTypes.Email, email),
                                };

                                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                var authPorperties = new AuthenticationProperties
                                {
                                    IsPersistent = true,
                                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                                };

                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    authPorperties
                                    );
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "E-mail ou senha incorretos.");
                                return View(loginClient);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao conectar: {ex.Message}");
                return View(loginClient);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}