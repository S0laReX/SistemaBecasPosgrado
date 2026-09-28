using SistemaBecasWeb.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
// La practica se ejecuta sin permisos de escritura en el Event Log de Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Agregar servicios de MVC
builder.Services.AddControllersWithViews();

// Inyección de Dependencias: Registrar el Repositorio de Oracle
builder.Services.AddScoped<ISolicitudRepository, OracleSolicitudRepository>();
builder.Services.AddScoped<TransaccionRepository>();
builder.Services.AddScoped<ReprogramarConvocatoriaRepository>();
builder.Services.AddScoped<CorreoPostulanteRepository>();
builder.Services.AddScoped<TransferenciaPostulacionRepository>();

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
    pattern: "{controller=Transacciones}/{action=Index}/{id?}");

app.Run();
