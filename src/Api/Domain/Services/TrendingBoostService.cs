using System;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Services;

public class TrendingBoostService
{
    private readonly AppDbContext _db;
    public TrendingBoostService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Recomendacion>> BoostAsync(
        IReadOnlyList<Recomendacion> items, float alpha, CancellationToken ct)
    {
        var ids = items.Select(i => i.idPelicula).ToArray();
        var trend = await _db.Trends.Where(t => ids.Contains(t.IdPelicula)).ToListAsync(ct);

        var boosted = items.Select(i =>
        {
            var hot = trend.FirstOrDefault(t => t.IdPelicula == i.idPelicula)?.GlobalHotness ?? 0f;
            return i with { score = i.score + alpha * hot };
        }).OrderByDescending(x => x.score).ToList();

        return boosted;
    }
}
