using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using music_shed.Servicos;
using music_shed.Infraestrutura;
using music_shed.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Adiciona autenticação com Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});

// Registro de serviços e repositórios
builder.Services.AddScoped<ServicoDeHash>();
builder.Services.AddScoped<IConnectionFactory, ConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

// Configure o pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();