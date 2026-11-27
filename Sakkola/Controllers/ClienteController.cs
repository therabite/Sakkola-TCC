using Microsoft.AspNetCore.Mvc;
using Sakkola.Models;
using Sakkola.Models.ViewModel;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Repository.Contract.Client;
using Sakkola.Repository.Interfaces;
using System.Security.Claims;

namespace Sakkola.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IUserClientRepository _clientRepository;

        public ClienteController(IPedidoRepository pedidoRepository, IUserClientRepository clientRepository)
        {
            _pedidoRepository = pedidoRepository;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<IActionResult> MeusPedidos()
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var pedidos = await _pedidoRepository.ObterPedidosPorUsuarioAsync(userId);
            return View(pedidos);
        }
        [HttpPost]
        public async Task<IActionResult> CancelarPedido(int idPedido)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _pedidoRepository.AtualizarStatusPedidoAsync(idPedido, "Cancelado");
            TempData["SucessoCliente"] = "Pedido cancelado com sucesso.";
            return RedirectToAction("MeusPedidos", "Carrinho");
        }
        [HttpPost]
        public async Task<IActionResult> AtualizarDados(Client model)
        {
            model.Email = User.Identity?.Name;

            if (!ModelState.IsValid)
            {
                return View("Perfil", model);
            }
            await _clientRepository.AtualizarClienteAsync(model);

            TempData["MensagemSucesso"] = "Dados atualizados com sucesso!";
            return RedirectToAction("Perfil");
        }

    }
}
