using Microsoft.AspNetCore.Mvc;
using Sakkola.Repository.Contract;
using System.Security.Claims;

namespace Sakkola.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IPedidoRepository _pedidoRepository;

        public ClienteController(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
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
    }
}
