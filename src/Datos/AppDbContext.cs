using System;
using Microsoft.EntityFrameworkCore;
using src.Modelos;

namespace src.Datos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opciones)
        : base(opciones)
    {
    }


    public DbSet<Pelicula> Peliculas { get; set; }
    public DbSet<Recomendacion> Recomendaciones { get; set; }
    public DbSet<PerfilUsuario> PerfilesUsuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        base.OnModelCreating(modelo);

        // Configuración de Pelicula
        modelo.Entity<Pelicula>()
            .HasKey(p => p.Id);

        // Como las Etiquetas son una lista, podemos mapearlas como JSON (si usas EFCore 7+ con SQLite o PostgreSQL)
        modelo.Entity<Pelicula>()
            .Property(p => p.Etiquetas)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        // Configuración de Recomendacion
        modelo.Entity<Recomendacion>()
            .HasKey(r => r.Id); 

        modelo.Entity<Recomendacion>()
            .HasOne(r => r.Pelicula)
            .WithMany()
            .HasForeignKey(r => r.PeliculaId);

        // Configuración de PerfilUsuario
        modelo.Entity<PerfilUsuario>()
            .HasKey(u => u.NombreUsuario);

        // GenerosPreferidos y EtiquetasPreferidas son HashSet se convierten a cadena
        modelo.Entity<PerfilUsuario>()
            .Property(u => u.GenerosPreferidos)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase)
            );

        modelo.Entity<PerfilUsuario>()
            .Property(u => u.EtiquetasPreferidas)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase)
            );

        // Ids vistos y Mi lista también como cadenas separadas
        modelo.Entity<PerfilUsuario>()
            .Property(u => u.IdsVistos)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList()
            );

        modelo.Entity<PerfilUsuario>()
            .Property(u => u.IdsMiLista)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet()
            );
    }
}

