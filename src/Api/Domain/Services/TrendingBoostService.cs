using System;
using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Services;

public class TrendingBoostService
{
    private readonly AppDbContext _db;
    public TrendingBoostService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Recommendation>> BoostAsync(
        IReadOnlyList<Recommendation> items, float alpha, CancellationToken ct)
    {
        var ids = items.Select(i => i.MovieId).ToArray();
        var trend = await _db.Trends.Where(t => ids.Contains(t.MovieId)).ToListAsync(ct);

        var boosted = items.Select(i =>
        {
            var hot = trend.FirstOrDefault(t => t.MovieId == i.MovieId)?.GlobalHotness ?? 0f;
            return i with { Score = i.Score + alpha * hot };
        }).OrderByDescending(x => x.Score).ToList();

        return boosted;
    }
}
