
using Microsoft.EntityFrameworkCore;
using src.Datos;
using src.Modelos;
using src.Servicios;
using src.Servicios.AlgoritmoEspeculativo;


var factory = new AppDbContextoFactory();
using var contexto = factory.CreateDbContext(args);


contexto.Database.EnsureCreated();

// 1. Obtener películas desde la base de datos
var peliculas = await contexto.Peliculas.ToListAsync();

// 2. Crear el motor de recomendación con el corpus de películas
var motor = new MotorRecomendacion(peliculas);

// 3. Crear una sesión de usuario (esto depende de cómo lo manejes tú)
var sesion = new SesionUsuario("Pedro Navaja");

// 4. Instanciar la interfaz de usuario con todo listo
InterfazUsuario interfaz = new InterfazUsuario(peliculas, motor, sesion);
