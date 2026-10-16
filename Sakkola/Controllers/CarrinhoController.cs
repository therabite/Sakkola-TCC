using Microsoft.AspNetCore.Mvc;
using Sakkola.Repository.Interfaces;
using System.Security.Claims;

namespace Sakkola.Controllers
{
    public class CarrinhoController : Controller
    {
        private readonly ICarrinhoRepository _carrinhoRepository;
        public CarrinhoController(ICarrinhoRepository carrinhoRepository)
        {
            _carrinhoRepository = carrinhoRepository;
        }
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var carrinho = await _carrinhoRepository.ObterCarrinhoPorUsuarioAsync(userId);
            return View(carrinho);
        }
        [HttpPost]
        public async Task<IActionResult> Adicionar(int idProduto, int quantidade = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _carrinhoRepository.AdicionarItemAsync(userId, idProduto, quantidade);

            // Se for chamada via AJAX/Fetch
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remover(int idItem)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _carrinhoRepository.RemoverItemAsync(userId, idItem);
            return RedirectToAction("Index");
        }
    }
}
