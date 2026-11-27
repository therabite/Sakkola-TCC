using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sakkola.Libraries.Filtro;
using Sakkola.Models.ViewModel;
using Sakkola.Models.ViewProdutos;
using Sakkola.Repository;
using Sakkola.Repository.Contract;
using Sakkola.Repository.Contract.Client;
using Sakkola.Repository.Contract.ColaboradorContract;
using Sakkola.Repository.Interfaces;

namespace Sakkola.Controllers
{
    public class ColaboradorController : Controller
    {
        private readonly IColaboradorRepository _colaboradorRepository;
        private readonly IPedidoRepository _pedidoRepository;

        public ColaboradorController(IColaboradorRepository colaboradorRepository, IPedidoRepository pedidoRepository)
        {
            _colaboradorRepository = colaboradorRepository;
            _pedidoRepository = pedidoRepository;
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
            HttpContext.Session.SetString("ColaboradorAdmin", colaborador.Administrador ? "True" : "False");
            HttpContext.Session.SetString("IdColaborador", colaborador.Id_colaborador.ToString());

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
        public async Task<IActionResult> PainelPedidos(string status)
        {
            ViewBag.StatusSelecionado = status;

            var pedidos = string.IsNullOrEmpty(status) || status == "Todos"
                ? await _pedidoRepository.ObterPedidosPorStatusOuTodosAsync(null)
                : await _pedidoRepository.ObterPedidosPorStatusOuTodosAsync(status);

            return View(pedidos);
        }
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("EmailColaborador");

            return RedirectToAction("Login", "Colaborador");
        }
        [ColaboradorAutorizacao]
        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int idPedido, string novoStatus)
        {
            if (idPedido <= 0 || string.IsNullOrWhiteSpace(novoStatus))
            {
                TempData["Erro"] = "Dados inválidos para atualizar o status do pedido.";
                return RedirectToAction(nameof(PainelPedidos));
            }
            var statusPermitidos = new[] { "Pendente", "Pago", "Aprovado", "Cancelado" };
            if (!statusPermitidos.Contains(novoStatus, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Erro"] = "Status de pedido não permitido.";
                return RedirectToAction(nameof(PainelPedidos));
            }

            await _pedidoRepository.AtualizarStatusPedidoAsync(idPedido, novoStatus);

            TempData["Sucesso"] = novoStatus.Equals("Cancelado", StringComparison.OrdinalIgnoreCase)
                ? $"Pedido #{idPedido} cancelado com sucesso."
                : $"Pedido #{idPedido} atualizado para '{novoStatus}'.";

            return RedirectToAction(nameof(PainelPedidos));
        }
        [HttpGet]
        public async Task<IActionResult> CadastrarProduto()
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }

            var categorias = await _colaboradorRepository.ObterCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CadastrarProduto(ProdutoCadastrarViewModel model, [FromServices] IWebHostEnvironment env)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int idProduto = await _colaboradorRepository.InserirProdutoAsync(model);

            if (model.Imagem != null && model.Imagem.Length > 0 && idProduto > 0)
            {
                string pastaDestino = Path.Combine(env.WebRootPath, "img", "img-produtos");

                if (!Directory.Exists(pastaDestino))
                {
                    Directory.CreateDirectory(pastaDestino);
                }

                string caminhoArquivo = Path.Combine(pastaDestino, $"{idProduto}.png");

                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await model.Imagem.CopyToAsync(stream);
                }
            }

            TempData["Sucesso"] = "Produto cadastrado com sucesso!";
            return RedirectToAction("CadastrarProduto");
        }
        [HttpGet]
        public async Task<IActionResult> GerenciarProdutos(string termoBusca, string categoria)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }
            ViewBag.TermoBusca = termoBusca;
            ViewBag.CategoriaSelecionada = categoria;

            var categorias = await _colaboradorRepository.ObterCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias, categoria);

            var produtos = await _colaboradorRepository.ObterTodosProdutosAsync(termoBusca, categoria);
            return View(produtos);
        }
        [HttpPost]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }

            await _colaboradorRepository.DeletarProdutoAsync(id);
            TempData["Sucesso"] = "Produto deletado com sucesso!";
            return RedirectToAction("GerenciarProdutos");
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }

            var produto = await _colaboradorRepository.ObterProdutoPorIdAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            produto.IdProduto = id;
            var categorias = await _colaboradorRepository.ObterCategoriasAsync();
            ViewBag.Categorias = new SelectList(categorias, produto.Categoria);

            return View(produto);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(int id, ProdutoCadastrarViewModel model, [FromServices] IWebHostEnvironment env)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }
            model.IdProduto = id;

            await _colaboradorRepository.AtualizarProdutoAsync(model);

            if (model.Imagem != null && model.Imagem.Length > 0)
            {
                string pastaDestino = Path.Combine(env.WebRootPath, "img", "img-produtos");
                if (!Directory.Exists(pastaDestino))
                {
                    Directory.CreateDirectory(pastaDestino);
                }

                string caminhoArquivo = Path.Combine(pastaDestino, $"{model.IdProduto}.png");
                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await model.Imagem.CopyToAsync(stream);
                }
            }

            TempData["Sucesso"] = "Produto atualizado com sucesso!";
            return RedirectToAction("GerenciarProdutos");
        }
        [HttpGet]
        public async Task<IActionResult> GerenciarUsuarios()
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }
            var usuarios = await _colaboradorRepository.ObterTodosUsuariosAsync();
            return View(usuarios);
        }
        [HttpPost]
        public async Task<IActionResult> EditarCliente(UsuarioViewModel model)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }

            await _colaboradorRepository.AtualizarUsuarioAsync(model);
            TempData["Sucesso"] = "Dados do usuário atualizados com sucesso!";
            return RedirectToAction("GerenciarUsuarios");
        }
        [HttpPost]
        public async Task<IActionResult> DeletarCliente(int id)
        {
            if (HttpContext.Session.GetString("ColaboradorAdmin") != "True")
            {
                return RedirectToAction("PainelPedidos", "Colaborador");
            }
            string idLogadoStr = HttpContext.Session.GetString("IdColaborador");
            if (!string.IsNullOrEmpty(idLogadoStr) && int.TryParse(idLogadoStr, out int idLogado) && idLogado == id)
            {
                HttpContext.Session.Clear();
            }

            await _colaboradorRepository.DeletarUsuarioAsync(id);
            TempData["Sucesso"] = "Usuário deletado com sucesso!";
            return RedirectToAction("GerenciarUsuarios");
        }
    }
}