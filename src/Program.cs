using Microsoft.EntityFrameworkCore;
using src.Datos;
using src.Modelos;
using src.Servicios;
using src.Servicios.AlgoritmoEspeculativo;

var factory = new AppDbContextoFactory();
await using var contexto = factory.CreateDbContext(args);

await contexto.Database.EnsureCreatedAsync();


var peliculas = await contexto.Peliculas
    .AsNoTracking()
    .ToListAsync();


var motor  = new MotorRecomendacion(peliculas);
var sesion = new SesionUsuario("Pedro Navaja");


var interfaz = new InterfazUsuario(peliculas, motor, sesion);


await interfaz.EjecutarAsync();
