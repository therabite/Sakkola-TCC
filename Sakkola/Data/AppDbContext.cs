using Microsoft.EntityFrameworkCore;

namespace Sakkola.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Models.Usuario> tbUser { get; set; }
        public DbSet<Models.Client> tbClient { get; set; }
        public DbSet<Models.Endereco> tbAddress { get; set; }
    }
}
