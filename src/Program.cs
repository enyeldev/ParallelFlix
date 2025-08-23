using Microsoft.EntityFrameworkCore;
using src.Datos;
using src.Modelos;
using src.Servicios;
using src.Servicios.AlgoritmoEspeculativo;

var factory = new AppDbContextoFactory();
await using var contexto = factory.CreateDbContext(args);

await contexto.Database.EnsureCreatedAsync();

// 1) Cargar peliculas
var peliculas = await contexto.Peliculas
    .AsNoTracking()
    .ToListAsync();

// 2) Motor y sesión
var motor  = new MotorRecomendacion(peliculas);
var sesion = new SesionUsuario("Pedro Navaja");

// 3) UI
var interfaz = new InterfazUsuario(peliculas, motor, sesion);

// *** FALTA ESTO ***
// Lanza el loop de la interfaz (bloquea hasta salir)
await interfaz.EjecutarAsync();
