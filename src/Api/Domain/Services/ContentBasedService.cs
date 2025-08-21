using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Api.Data;
using Api.Domain.Parallel;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Services;

public class ContentBasedService : IAlgoritmoRecomendacion
{
    private readonly AppDbContext _db;
    private readonly PipelineBarriers _barriers;
    public string Nombre => "ContentBased";

    public ContentBasedService(AppDbContext db, PipelineBarriers barriers)
    {
        _db = db; _barriers = barriers;
    }

    public async Task<RecResultado> RecomendadoAsync(int idUser, int k, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();


        var userRatings = await _db.Ratings.Where(r => r.IdUsuario == idUser).ToListAsync(ct);
        var movieGenres = await _db.GenerosPelicula.Include(mg => mg.Genero).ToListAsync(ct);
        var movies = await _db.Movies.AsNoTracking().ToListAsync(ct);

        // Mapeos
        var gIds = movieGenres.Select(mg => mg.IdGenero).Distinct().OrderBy(x => x).ToArray();
        var gIndex = gIds.Select((g, i) => (g, i)).ToDictionary(t => t.g, t => t.i);
        var dim = gIds.Length;


        var movieVec = new Dictionary<int, float[]>();
        foreach (var m in movies)
        {
            var vec = new float[dim];
            foreach (var mg in movieGenres.Where(x => x.IdPelicula == m.Id))
            {
                vec[gIndex[mg.IdGenero]] = 1f;
            }
            movieVec[m.Id] = vec;
        }

        // Preprocesamiento listo
        _barriers.PreprocessBarrier.SignalAndWait();

        // 3) Perfil del usuario = suma ponderada de vectores de películas vistas por su rating
        var userProfile = new float[dim];
        foreach (var r in userRatings)
        {
            var v = movieVec[r.IdPelicula];
            for (int i = 0; i < dim; i++) userProfile[i] += r.Score * v[i];
        }

        // 4) Scoring por coseno en paralelo sobre todas las películas no vistas
        var seen = userRatings.Select(r => r.IdPelicula).ToHashSet();
        var scores = new ConcurrentDictionary<int, float>();

        System.Threading.Tasks.Parallel.ForEach(movies, new ParallelOptions { CancellationToken = ct }, m =>
        {
            if (seen.Contains(m.Id)) return;
            var v = movieVec[m.Id];
            float dot = 0f, a = 0f, b = 0f;
            for (int i = 0; i < dim; i++)
            {
                dot += userProfile[i] * v[i];
                a += userProfile[i] * userProfile[i];
                b += v[i] * v[i];
            }
            var denom = MathF.Sqrt(a) * MathF.Sqrt(b);
            var sim = denom == 0 ? 0f : dot / denom;
            if (sim > 0) scores[m.Id] = sim;
        });

        _barriers.SimilarityBarrier.SignalAndWait();

        ct.ThrowIfCancellationRequested();

        var top = scores.OrderByDescending(kv => kv.Value).Take(k).Select(kv => kv.Key).ToArray();
        var meta = movies.Where(m => top.Contains(m.Id)).ToList();
        var result = top.Select(mId =>
        {
            var title = meta.First(m => m.Id == mId).Titulo;
            return new Recomendacion(mId, title, scores[mId]);
        }).ToList();

        sw.Stop();
        return new RecResultado(Nombre, sw.Elapsed, result);
    }
}
