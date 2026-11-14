using Microsoft.AspNetCore.Mvc;
using Sakkola.Models;
using Sakkola.Models.ViewCarrinho;
using Sakkola.Repository;
using Sakkola.Repository.Contract;
using Sakkola.Repository.Interfaces;
using System.Security.Claims;

namespace Sakkola.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoRepository _carrinhoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        public CarrinhoController(ICarrinhoRepository carrinhoRepository, IPedidoRepository pedidoRepository)
        {
            _carrinhoRepository = carrinhoRepository;
            _pedidoRepository = pedidoRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var carrinho = await _carrinhoRepository.ObterCarrinhoPorUsuarioAsync(userId.ToString());
            if (carrinho == null)
            {
                carrinho = new Sakkola.Models.ViewCarrinho.Carrinho();
            }
            
            return View(carrinho);
        }
        [HttpPost]
        public async Task<IActionResult> Adicionar(int idProduto, int quantidade = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    requiresLogin = true,
                    message = "Você precisa estar logado para adicionar produtos ao carrinho!",
                    redirect = Url.Action("Login", "Account")
                });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity.Name;

            await _carrinhoRepository.AdicionarItemAsync(userId, idProduto, quantidade);
            var carrinho = await _carrinhoRepository.ObterCarrinhoPorUsuarioAsync(userId);

            return Json(new { success = true, totalItens = carrinho.TotalItens });
        }
        [HttpPost]
        public async Task<IActionResult> AtualizarQuantidade(int idItem, int quantidade)
        {
            try
            {
                await _carrinhoRepository.AtualizarQuantidadeItemAsync(idItem, quantidade);

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Remover(int idItem)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _carrinhoRepository.RemoverItemAsync(userId, idItem);
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarEndereco(Carrinho carrinho)
        {
            string userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login"); 
            }
            await _carrinhoRepository.AtualizarDadosEntregaAsync(userIdStr, carrinho);
           
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> SelecionarPagamento()
        {
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(userIdClaim);
            var itens = await _carrinhoRepository.ObterItensPorUsuarioAsync(userId);

            if (itens == null || !itens.Any())
            {
                TempData["Erro"] = "O seu carrinho está vazio!";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarFinalizacao(string formaPagamento)
        {
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");
            int userId = Convert.ToInt32(userIdClaim);

            var itens = await _carrinhoRepository.ObterItensPorUsuarioAsync(userId);
            if (itens == null || !itens.Any()) return RedirectToAction("Index");

            decimal subtotal = itens.Sum(i => i.PrecoUnitario * i.Quantidade);
            decimal valorFrete = 10.00m;
            decimal valorTotalGeral = subtotal + valorFrete;

            string statusInicial = (formaPagamento == "PIX") ? "Pago" : "Pendente";

            await _carrinhoRepository.FinalizarCompraAsync(userId, valorTotalGeral, formaPagamento, statusInicial);

            TempData["SucessoCliente"] = $"Pedido realizado com sucesso! O status encontra-se: {statusInicial}.";

            return RedirectToAction("MeusPedidos", "Carrinho"); 
        }

        [HttpGet]
        public async Task<IActionResult> MeusPedidos()
        {
            string userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");
            int userId = Convert.ToInt32(userIdClaim);

            var pedidos = await _pedidoRepository.ObterPedidosPorUsuarioAsync(userId);
            return View(pedidos);
        }
    }
}
