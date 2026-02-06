using Microsoft.EntityFrameworkCore;
using BlackJackMVC.Data;
using BlackJackMVC.Services;

var constructor = WebApplication.CreateBuilder(args);


constructor.Services.AddControllersWithViews();

// Esto pa q sirva el login
constructor.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromHours(24);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

// EF = Sqlite
constructor.Services.AddDbContext<ContextoBaseDatos>(opciones =>
    opciones.UseSqlite(constructor.Configuration.GetConnectionString("ConexionSQLite")));

// Registrar los servicios
constructor.Services.AddScoped<IServicioJuego, ServicioJuego>();
constructor.Services.AddScoped<IServicioTienda, ServicioTienda>();
constructor.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
constructor.Services.AddScoped<IServicioAdmin, ServicioAdmin>();

var aplicacion = constructor.Build();

// Esto es pa crear la base de datos automaticamente si no existe
using (var scope = aplicacion.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ContextoBaseDatos>();
    contexto.Database.EnsureCreated();
}


// Recordatorio pa el orden correcto pa evitar errores (ya me a pasado D: )
// 1
aplicacion.UseStaticFiles();

// 2
aplicacion.UseRouting();

// 3 habilitar las sesiones
aplicacion.UseSession();

// 4
aplicacion.MapControllerRoute(
    name: "default",
    pattern: "{controller=ControladorAutenticacion}/{action=Login}/{id?}");

// 5
aplicacion.Run();