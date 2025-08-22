using src.Modelos;
using src.Nucleo;
using System.Collections.Concurrent;

namespace src.Recomendadores;

public sealed class RecomendadorContenido : IRecomendador
{
    public string Nombre => "Contenido";

    public Task<List<Recomendacion>> RecomendarAsync(PerfilUsuario usuario, List<Pelicula> corpus, int k, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            var vistos = corpus.Where(m => usuario.IdsVistos.Contains(m.Id)).ToList();
            var candidatos = corpus.Where(m => !usuario.IdsVistos.Contains(m.Id)).ToList();

            var bolsa = new ConcurrentBag<Recomendacion>();
            Parallel.ForEach(candidatos, new ParallelOptions{ CancellationToken = ct }, m =>
            {
                double puntuacionBase = 0;
                if (usuario.GenerosPreferidos.Contains(m.Genero)) puntuacionBase += 0.5;
                puntuacionBase += m.Calificacion * 0.05;

                double simAlUltimo = 0;
                var ultimoId = usuario.IdsVistos.LastOrDefault();
                if (ultimoId != 0)
                {
                    var ultimo = corpus.First(x => x.Id == ultimoId);
                    simAlUltimo = Similitud.SimilitudCacheada(m, ultimo);
                }

                double impulsoEtiquetas = usuario.EtiquetasPreferidas.Count == 0 ? 0 :
                    (double) m.Etiquetas.Count(t => usuario.EtiquetasPreferidas.Contains(t)) / Math.Max(1, usuario.EtiquetasPreferidas.Count);

                double puntuacion = puntuacionBase + 0.6 * simAlUltimo + 0.2 * impulsoEtiquetas;
                bolsa.Add(new Recomendacion{ Pelicula = m, Puntuacion = puntuacion });
            });

            return bolsa.OrderByDescending(r => r.Puntuacion).ThenByDescending(r => r.Pelicula.Calificacion).Take(k).ToList();
        }, ct);
    }
}
