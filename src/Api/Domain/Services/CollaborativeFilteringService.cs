using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using Api.Data;
using Api.Domain.Parallel;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Services;

public class CollaborativeFilteringService : IAlgoritmoRecomendacion
{
    private readonly AppDbContext _db;
    private readonly SimilarityMatrixCache _cache;
    private readonly PipelineBarriers _barriers;
    public string Nombre => "CollaborativeFiltering";

    public CollaborativeFilteringService(AppDbContext db, SimilarityMatrixCache cache, PipelineBarriers barriers)
    {
        _db = db; _cache = cache; _barriers = barriers;
    }

    public async Task<RecResultado> RecomendadoAsync(int idUser, int k, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        // 1) Cargar ratings a memoria (subset para demo; en real usar particionado por segmentos)
        var ratings = await _db.Ratings.AsNoTracking().ToListAsync(ct);

        // 2) Construir matriz usuario->(movie,score)
        var byUser = ratings.GroupBy(r => r.IdUsuario)
                            .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.IdPelicula, r => r.Score));
        var users = byUser.Keys.ToArray();
        var userIndex = users.Select((u, i) => (u, i)).ToDictionary(t => t.u, t => t.i);
        var movies = ratings.Select(r => r.IdPelicula).Distinct().ToArray();

        // 3) Matriz de similitud de usuarios (coseno) – calculada en paralelo y cacheada
        var key = $"userSim_{users.Length}";
        var sim = _cache.GetOrAdd(key, () => new float[users.Length, users.Length]);

        // Preprocesamiento: normalización (señalamos barrera)
        _barriers.PreprocessBarrier.SignalAndWait();

        // Similaridad en paralelo
        var partitioner = Partitioner.Create(0, users.Length, Math.Max(1, users.Length / Environment.ProcessorCount));
        System.Threading.Tasks.Parallel.ForEach(partitioner, new ParallelOptions { CancellationToken = ct }, range =>
        {
            for (int i = range.Item1; i < range.Item2; i++)
            {
                var ui = users[i];
                var vi = byUser[ui];
                for (int j = i + 1; j < users.Length; j++)
                {
                    var uj = users[j];
                    var vj = byUser[uj];
                    // coseno(ui, uj)
                    float dot = 0f, a = 0f, b = 0f;
                    // iterar intersección rápida
                    foreach (var kv in vi)
                    {
                        if (vj.TryGetValue(kv.Key, out var s2))
                        {
                            var s1 = kv.Value;
                            dot += s1 * s2; a += s1 * s1; b += s2 * s2;
                        }
                    }
                    var denom = MathF.Sqrt(a) * MathF.Sqrt(b);
                    var c = denom == 0 ? 0f : dot / denom;
                    sim[i, j] = c; sim[j, i] = c;
                }
            }
        });

        _barriers.SimilarityBarrier.SignalAndWait();

        // 4) Vecinos + predicción de ratings para películas no vistas
        var targetIdx = userIndex[idUser];
        var seen = byUser[idUser].Keys.ToHashSet();
        var scores = new ConcurrentDictionary<int, float>(); // movieId -> score acumulado

        System.Threading.Tasks.Parallel.ForEach(movies, new ParallelOptions { CancellationToken = ct }, mId =>
        {
            if (seen.Contains(mId)) return;
            float num = 0f, den = 0f;
            for (int j = 0; j < users.Length; j++)
            {
                if (j == targetIdx) continue;
                var uj = users[j];
                if (byUser[uj].TryGetValue(mId, out var rj))
                {
                    var w = sim[targetIdx, j];
                    if (w <= 0) continue;
                    num += w * rj;
                    den += MathF.Abs(w);
                }
            }
            if (den > 0)
            {
                scores[mId] = num / den;
            }
        });

        ct.ThrowIfCancellationRequested();

        // 5) Top-K
        var top = scores.OrderByDescending(kv => kv.Value).Take(k)
                        .Select(kv => kv.Key).ToArray();
        var meta = await _db.Movies.Where(m => top.Contains(m.Id)).ToListAsync(ct);
        var result = top.Select(mId =>
        {
            var title = meta.First(m => m.Id == mId).Titulo;
            var sc = scores[mId];
            return new Recomendacion(mId, title, sc);
        }).ToList();

        sw.Stop();
        return new RecResultado(Nombre, sw.Elapsed, result);
    }
}
