using src.Modelos;
using System.Collections.Concurrent;

namespace src.Recomendadores;

public sealed class RecomendadorTendencias : IRecomendador
{
    public string Nombre => "Tendencias";

    public Task<List<Recomendacion>> RecomendarAsync(PerfilUsuario usuario, List<Pelicula> corpus, int k, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var rnd = new Random(DateTime.UtcNow.Minute + usuario.NombreUsuario.Length);
            var bolsa = new ConcurrentBag<Recomendacion>();

            Parallel.ForEach(corpus, new ParallelOptions{ CancellationToken = ct }, m =>
            {
                var ruido = rnd.NextDouble() * 0.5;
                var puntuacion = m.Calificacion + ruido;
                bolsa.Add(new Recomendacion{ Pelicula = m, Puntuacion = puntuacion });
            });

            return bolsa.Where(r => !usuario.IdsVistos.Contains(r.Pelicula.Id))
                      .OrderByDescending(r => r.Puntuacion)
                      .Take(k)
                      .ToList();
        }, ct);
    }
}
