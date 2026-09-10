using Sakkola.Services;
// Adicione o uso do Entity Framework
using Microsoft.EntityFrameworkCore;
using Sakkola.Data;

// 1. Registra o AppDbContext informando a conexão com o MySQL
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUsuarioServices, UsuarioServices>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Registra o serviço de usuário (mantenha como já está)
builder.Services.AddScoped<IUsuarioServices, UsuarioServices>();


// Configura Autenticação via Cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "Sakkola.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

var app = builder.Build();

// Adicione na ordem de execução do pipeline:
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // 1º Identifica o usuário
app.UseAuthorization();  // 2º Controla permissões de acesso

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();