using SistemaBecasWeb.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de MVC
builder.Services.AddControllersWithViews();

// Inyección de Dependencias: Registrar el Repositorio de Oracle
builder.Services.AddScoped<ISolicitudRepository, OracleSolicitudRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Solicitud}/{action=Index}/{id?}");

app.Run();
