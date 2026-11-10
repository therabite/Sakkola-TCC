using Microsoft.AspNetCore.Mvc;
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
            if (string.IsNullOrEmpty(model.Email) || !model.Email.EndsWith("@admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Acesso restrito. O e-mail deve pertencer ao domínio @admin.");
                return View(model);
            }

            var colaborador = await _colaboradorRepository.ValidarColaboradorAsync(model.Email, model.Senha);

            if (colaborador == null)
            {
                ModelState.AddModelError(string.Empty, "E-mail ou senha de colaborador inválidos.");
                return View(model);
            }

            HttpContext.Session.SetString("EmailColaborador", colaborador.Email);
            HttpContext.Session.SetString("SenhaColaborador", colaborador.Senha);

            return RedirectToAction("PainelPedidos", "Colaborador");
        }
        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(RegisterColaborador model)
        {
            if (string.IsNullOrEmpty(model.email) || !model.email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "O e-mail deve terminar com @admin.com");
                return View(model);
            }

            try
            {
                await _colaboradorRepository.CadastrarFuncionarioAsync(
                    model.nome,
                    model.data_nasc,
                    model.telefone,
                    model.cpf,
                    model.email,
                    model.senha,
                    model.IsAdmin
                );

                TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro: " + ex.Message);
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> PainelPedidos()
        {
            if (HttpContext.Session.GetString("EmailColaborador") == null)
            {
                return RedirectToAction("Login", "Colaborador");
            }

            var pedidos = await _colaboradorRepository.ObterPedidosPendentesAsync();
            return View(pedidos);
        }
        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int idPedido, string novoStatus)
        {
            await _colaboradorRepository.AtualizarStatusPedidoAsync(idPedido, novoStatus);

            TempData["Sucesso"] = "Status do pedido atualizado com sucesso!";
            return RedirectToAction(nameof(PainelPedidos));
        }
    }
}
