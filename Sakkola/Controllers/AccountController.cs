
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Sakkola.Models.ViewModel;
using Sakkola.Services;

namespace Sakkola.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsuarioServices _usuarioServices;

        public AccountController(IUsuarioServices usuarioServices)
        {
            _usuarioServices = usuarioServices;
        }

        [HttpGet]
        public IActionResult Login(){ 
           return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginClient model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _usuarioServices.ValidarCredenciaisAsync(model.email, model.senha);
            if(usuario == null)
            {
                ModelState.AddModelError(string.Empty, "email ou senha inválidos");
                return View(model);
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.id_user.ToString()),
                new Claim(ClaimTypes.Name, usuario.nome.ToString()),
                new Claim(ClaimTypes.Email, usuario.email.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            var autoProperties = new AuthenticationProperties
            {
                IsPersistent = model.lembraDeMim
            };

            await HttpContext.SignInAsync("CookieAuth", principal, autoProperties);

            return RedirectToAction("Index", "Home"); // mudar para view do carrinho posteriormente
        }

        [HttpGet]
        public IActionResult Cadastro() => View("Cadastro");

        [HttpPost]
        public async Task<IActionResult> Cadastro(RegisterClient model)
        {
            if(!ModelState.IsValid) return View(model);
            if(await _usuarioServices.EmailJaCadastradoAsync(model.email))
            {
                ModelState.AddModelError("Email", "este email já está em uso.");
                return View("Cadastro", model);
            }

            await _usuarioServices.CadastrarClienteAsync(model);
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }
}
