using Microsoft.EntityFrameworkCore;
using Sakkola.Models;

namespace Sakkola.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Client> Clientes { get; set; }
        
        public DbSet<Endereco> Enderecos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().ToTable("tbClient");
            modelBuilder.Entity<Endereco>().ToTable("tbAddress");
            modelBuilder.Entity<Usuario>().ToTable("tbUser");
        }
    }
}
