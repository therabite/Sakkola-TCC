using Microsoft.EntityFrameworkCore;
using Sakkola.Models.ViewModel;

namespace Sakkola.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Models.Usuario> tbUser { get; set; }
        public DbSet<Client> tbClient { get; set; }
        public DbSet<Models.Endereco> tbAddress { get; set; }
    }
}
