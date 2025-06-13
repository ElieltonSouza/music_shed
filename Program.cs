using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using music_shed.Servicos;
using music_shed.Infraestrutura;
using music_shed.Repositorios;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<ServicoDeHash>();
builder.Services.AddScoped<IConnectionFactory, ConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IProfessorRepositorio, ProfessorRepositorio>();
builder.Services.AddScoped<IAgendaRepositorio, AgendaRepositorio>();
builder.Services.AddScoped<IAlunoPorProfessorRepositorio, AlunoPorProfessorRepositorio>();
builder.Services.AddScoped<ISolicitacaoReagendamentoRepositorio, SolicitacaoReagendamentoRepositorio>();
builder.Services.AddScoped<ICancelamentoAulaRepositorio, CancelamentoAulaRepositorio>();
builder.Services.AddScoped<IConfiguracaoGlobalRepositorio, ConfiguracaoGlobalRepositorio>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

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