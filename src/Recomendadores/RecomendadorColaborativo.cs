using src.Modelos;
using System.Collections.Concurrent;

namespace src.Recomendadores;

public sealed class RecomendadorColaborativo : IRecomendador
{
    public string Nombre => "Colaborativo";

    public Task<List<Recomendacion>> RecomendarAsync(PerfilUsuario usuario, List<Pelicula> corpus, int k, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var rnd = new Random(usuario.NombreUsuario.GetHashCode());
            var candidatos = corpus.Where(m => !usuario.IdsVistos.Contains(m.Id)).ToList();
            var bolsa = new ConcurrentBag<(Pelicula,double)>();

            Parallel.ForEach(candidatos, new ParallelOptions{ CancellationToken = ct }, m =>
            {
                double votoMasivo = 0;
                foreach (var v in usuario.IdsVistos.Take(10))
                {
                    var mult = 0.5 + rnd.NextDouble();
                    votoMasivo += mult * (m.Genero.Equals(corpus.First(x=>x.Id==v).Genero, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.2);
                }
                votoMasivo += m.Calificacion * 0.3;
                bolsa.Add((m, votoMasivo));
            });

            return bolsa.Select(t => new Recomendacion{ Pelicula=t.Item1, Puntuacion=t.Item2 })
                      .OrderByDescending(r => r.Puntuacion)
                      .Take(k)
                      .ToList();
        }, ct);
    }
}
