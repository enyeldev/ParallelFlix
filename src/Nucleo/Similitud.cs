using src.Modelos;
using System.Collections.Concurrent;

namespace src.Nucleo;

public static class Similitud
{
    // Caché concurrente compartido entre estrategias
    private static readonly ConcurrentDictionary<(int,int), double> _cacheSim = new();

    public static double JaccardGenero(Pelicula a, Pelicula b)
    {
        if (string.Equals(a.Genero, b.Genero, StringComparison.OrdinalIgnoreCase)) return 1.0;
        return 0.25; // géneros distintos => afinidad baja
    }

    public static double JaccardEtiquetas(Pelicula a, Pelicula b)
    {
        var sa = a.Etiquetas.Select(t => t.ToLower()).ToHashSet();
        var sb = b.Etiquetas.Select(t => t.ToLower()).ToHashSet();
        var inter = sa.Intersect(sb).Count();
        var union = sa.Union(sb).Count();
        return union == 0 ? 0 : (double)inter / union;
    }

    public static double SimilitudCacheada(Pelicula a, Pelicula b)
    {
        var clave = a.Id < b.Id ? (a.Id,b.Id) : (b.Id,a.Id);
        return _cacheSim.GetOrAdd(clave, _ => 0.6 * JaccardGenero(a,b) + 0.4 * JaccardEtiquetas(a,b));
    }
}
