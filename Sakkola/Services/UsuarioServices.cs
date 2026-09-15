using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sakkola.Data;
using Sakkola.Models;
using Sakkola.Models.ViewModel;

namespace Sakkola.Services
{
    public class UsuarioServices : IUsuarioServices
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _hasher;

        public UsuarioServices(AppDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<Usuario>();
        }
        public async Task<bool> EmailJaCadastradoAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.email == email);
        }
        public async Task CadastrarClienteAsync(RegisterClient model)
        {
            var novoEndereco = new Endereco
            {
                cep = int.Parse(model.cep.Replace("-", "").Trim()), 
                logradouro = "Não informado",
                num = 0,
                bairro = "Não informado",
                estado = "UF"
            };

            var cliente = new Client
            {
                name = model.name,
                email = model.email,
                cpf = model.cpf,
                rg = model.rg,
                telefone = model.telefone,
                dataNasc = model.dataNasc
            };

            cliente.senha = _hasher.HashPassword(cliente, model.senha);

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(); 
        }
        public async Task<Usuario?> ValidarCredenciaisAsync(string email, string senha)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.email == email);

            if (usuario == null) return null;

            // Verifica se a senha informada corresponde ao Hash armazenado
            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.senha, senha);
            return resultado == PasswordVerificationResult.Success ? usuario : null;
        }
    }
}
