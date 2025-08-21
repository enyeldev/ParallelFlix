using System;
using System.Collections.Concurrent;

namespace Api.Domain.Parallel;

public class SimilarityMatrixCache
{
    // Para demo: cache de matrices en memoria (clave por nombre)
    private readonly ConcurrentDictionary<string, float[,]> _matrices = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);

    public float[,] GetOrAdd(string key, Func<float[,]> factory)
    {
        if (_matrices.TryGetValue(key, out var existing)) return existing;
        _lock.EnterWriteLock();
        try
        {
            if (_matrices.TryGetValue(key, out existing)) return existing;
            var created = factory();
            _matrices[key] = created;
            return created;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool TryGet(string key, out float[,] matrix) => _matrices.TryGetValue(key, out matrix!);
}

