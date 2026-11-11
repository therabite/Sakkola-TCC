using Microsoft.AspNetCore.Mvc;
using Sakkola.Libraries.Filtro;
using Sakkola.Models.ViewModel;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Controllers
{
    public class ColaboradorController : Controller
    {
        private readonly IColaboradorRepository _colaboradorRepository;

        public ColaboradorController(IColaboradorRepository colaboradorRepository)
        {
            _colaboradorRepository = colaboradorRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginColaborador model)
        {
            if (string.IsNullOrEmpty(model.Email) || !model.Email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Acesso restrito. O e-mail deve pertencer ao domínio @admin.com");
                return View(model);
            }
            var colaborador = await _colaboradorRepository.ValidarColaboradorAsync(model.Email, model.Senha);

            if (colaborador == null)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha de colaborador inválidos.");
                return View(model);
            }

            HttpContext.Session.SetString("Senha", colaborador.Senha);
            HttpContext.Session.SetString("EmailColaborador", colaborador.Email);

            return RedirectToAction("PainelPedidos", "Colaborador");
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(RegisterColaborador colaborador)
        {
            if (!ModelState.IsValid) return View(colaborador);

            if (string.IsNullOrEmpty(colaborador.Email) || !colaborador.Email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "O e-mail deve terminar com @admin.com");
                return View(colaborador);
            }

            await _colaboradorRepository.CadastrarFuncionarioAsync(colaborador);

            TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
            return RedirectToAction("Login");
        }

        [ColaboradorAutorizacao]
        [HttpGet]
        public IActionResult PainelPedidos()
        {
            return View();
        }
    }
}
