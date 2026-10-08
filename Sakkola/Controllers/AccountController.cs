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
            if (!ModelState.IsValid)
            {
                return View(loginClient);
            }
            try
            {
                var usuaria = await _clientRepository.ValidarUserAsync(loginClient.Email, loginClient.Senha);

                if (usuaria == null)
                {
                    ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                    return View(loginClient);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuaria.Id_user.ToString()),
                    new Claim(ClaimTypes.Name, usuaria.nome),
                    new Claim(ClaimTypes.Email, usuaria.email)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}