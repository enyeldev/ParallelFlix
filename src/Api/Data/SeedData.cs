
using Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;
public static class SeedData {
    public static async Task EnsureSeedAsync(AppDbContext db) {
        if (await db.Users.AnyAsync()) return;

        var genres = new[] { "Action", "Drama", "Comedy", "Sci-Fi", "Romance", "Thriller" }
            .Select((n,i) => new Genre { Id = i+1, Nombre = n }).ToList();
        db.Genres.AddRange(genres);

        var rnd = new Random(42);
        var movies = Enumerable.Range(1, 500)
            .Select(i => new Movie { Id = i, Titulo = $"Movie {i}" })
            .ToList();
        db.Movies.AddRange(movies);

        var mgs = new List<GeneroPelicula>();
        foreach (var m in movies) {
            var gcount = rnd.Next(1, 4);
            var picked = genres.OrderBy(_ => rnd.Next()).Take(gcount).ToList();
            picked.ForEach(g => mgs.Add(new GeneroPelicula { IdPelicula = m.Id, IdGenero = g.Id }));
        }
        db.GenerosPelicula.AddRange(mgs);

        var users = Enumerable.Range(1, 5_000)
            .Select(i => new User { Id = i, Email = $"user{i}@demo.local" }).ToList();
        db.Users.AddRange(users);

        var ratings = new List<Rating>();
        foreach (var u in users) {
            var rated = movies.OrderBy(_ => rnd.Next()).Take(rnd.Next(30, 120)).ToList();
            rated.ForEach(m => ratings.Add(new Rating {
                IdUsuario = u.Id, IdPelicula = m.Id, Score = (float)(rnd.Next(1, 10) / 2.0),
                Timestamp = DateTime.UtcNow.AddDays(-rnd.Next(1, 365))
            }));
        }
        db.Ratings.AddRange(ratings);

        var trends = movies.Take(50).Select(m => new Trend {
            IdPelicula = m.Id, GlobalHotness = (float)rnd.NextDouble() * 2f, UpdatedAt = DateTime.UtcNow
        }).ToList();
        db.Trends.AddRange(trends);

        await db.SaveChangesAsync();
    }
}
