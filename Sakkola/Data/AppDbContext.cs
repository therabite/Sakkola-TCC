using Microsoft.EntityFrameworkCore;
using Sakkola.Models;

namespace Sakkola.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Client> Clientes { get; set; }
    }
}
