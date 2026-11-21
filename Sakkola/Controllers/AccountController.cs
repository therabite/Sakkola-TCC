using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
using Sakkola.Repository.Interfaces;
using System.Security.Claims;

namespace Sakkola.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserClientRepository _clientRepository;

        public AccountController(IUserClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var emailUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(emailUsuario))
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await _clientRepository.ObterPorEmailAsync(emailUsuario);

            if (cliente == null)
            {
                cliente = new Sakkola.Models.Client { Email = emailUsuario };
            }

            return View(cliente); 
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
            if (!ModelState.IsValid)
            {
                return View(registerClient);
            }
            try
            {
                bool emailExiste = await _clientRepository.UserExistsAsync(registerClient.email ?? "");
                if (emailExiste)
                {
                    ModelState.AddModelError("email", "Este email já está cadastrado.");
                    return View(registerClient);
                }

                await _clientRepository.CadastrarClienteAsync(registerClient, endereco);

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
            if (loginClient == null || string.IsNullOrEmpty(loginClient.Email))
            {
                return View(loginClient);
            }

            if (loginClient.Email.Contains("admin.com", StringComparison.OrdinalIgnoreCase))
            {
                TempData["MensagemAviso"] = "E-mail administrativo detectado. Por favor, faça login na área de colaboradores.";

                return RedirectToAction("Login", "Colaborador");
            }
            try
            {
                var usuario = await _clientRepository.ValidarUserAsync(loginClient.Email, loginClient.Senha);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return View(loginClient);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id_user.ToString()),
                    new Claim(ClaimTypes.Name, usuario.nome),
                    new Claim(ClaimTypes.Email, usuario.email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao validar o usuário: {ex.Message}");
                return View(loginClient);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarDados(Client model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                model.Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            }
            if (string.IsNullOrEmpty(model.Email))
            {
                return Unauthorized();
            }
            await _clientRepository.AtualizarClienteAsync(model);

            return Ok();
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