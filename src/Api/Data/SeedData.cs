namespace Api.Data;

using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

public static class DatosSemilla {
    public static async Task AsegurarSemillaAsync(ContextoAplicacion db) {
        if (await db.Usuarios.AnyAsync()) return;

        var generos = new[] { "Acción", "Drama", "Comedia", "Ciencia Ficción", "Romance", "Suspenso" }
            .Select((n,i) => new Genero { Id = i+1, Nombre = n }).ToList();
        db.Generos.AddRange(generos);

        var aleatorio = new Random(42);
        var peliculas = Enumerable.Range(1, 500)
            .Select(i => new Pelicula { Id = i, Titulo = $"Pelicula {i}" }).ToList();
        db.Peliculas.AddRange(peliculas);

        var enlacesPG = new List<PeliculaGenero>();
        foreach (var p in peliculas) {
            var cantidadG = aleatorio.Next(1, 4);
            var tomados = generos.OrderBy(_ => aleatorio.Next()).Take(cantidadG).ToList();
            tomados.ForEach(g => enlacesPG.Add(new PeliculaGenero { PeliculaId = p.Id, GeneroId = g.Id }));
        }
        db.PeliculaGeneros.AddRange(enlacesPG);

        var usuarios = Enumerable.Range(1, 5000)
            .Select(i => new Usuario { Id = i, Correo = $"usuario{i}@demo.local" }).ToList();
        db.Usuarios.AddRange(usuarios);

        var calificaciones = new List<Calificacion>();
        foreach (var u in usuarios) {
            var vistas = peliculas.OrderBy(_ => aleatorio.Next()).Take(aleatorio.Next(30, 120)).ToList();
            vistas.ForEach(p => calificaciones.Add(new Calificacion {
                UsuarioId = u.Id,
                PeliculaId = p.Id,
                Puntuacion = (float)(aleatorio.Next(1, 10) / 2.0),
                Momento = DateTime.UtcNow.AddDays(-aleatorio.Next(1, 365))
            }));
        }
        db.Calificaciones.AddRange(calificaciones);

        var tendencias = peliculas.Take(50).Select(p => new Tendencia {
            PeliculaId = p.Id,
            IntensidadGlobal = (float)aleatorio.NextDouble() * 2f,
            ActualizadoEn = DateTime.UtcNow
        }).ToList();
        db.Tendencias.AddRange(tendencias);

        await db.SaveChangesAsync();
    }
}

