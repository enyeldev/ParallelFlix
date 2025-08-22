using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;
using src.Recomendadores;
using src.Servicios;
using System.Diagnostics;

namespace src.Servicios.AlgoritmoEspeculativo
{
    public class MotorRecomendacion
    {
        private readonly List<Pelicula> _corpus;
        private readonly List<IRecomendador> _estrategias;
        public Metricas MetricasRendimiento { get; } = new Metricas();

        public MotorRecomendacion(List<Pelicula> corpus)
        {
            _corpus = corpus;
            _estrategias = new(){ new RecomendadorContenido(), new RecomendadorColaborativo(), new RecomendadorTendencias() };
        }

        // Descomposición especulativa: lanzar todas las estrategias y tomar la que responde primero, con fusión sencilla.
        public async Task<(List<Recomendacion> resultados, Dictionary<string,TimeSpan> tiempos)> RecomendarEspeculativoAsync(PerfilUsuario usuario, int k)
        {
            using var cts = new CancellationTokenSource();
            var mapaSw = new Dictionary<string, Stopwatch>();
            var tareas = _estrategias.Select(s => EjecutarMedido(s, usuario, k, cts.Token, mapaSw)).ToList();

            var primero = await Task.WhenAny(tareas);
            // Cancelamos los que tarden demasiado (especulativo)
            cts.Cancel();

            var resultadoPrimero = await primero;

            // Recolectar parciales completadas y fusionar top-k (mejor esfuerzo)
            var combinados = new List<Recomendacion>(resultadoPrimero.resultados);
            foreach (var t in tareas)
            {
                if (t.IsCompletedSuccessfully && t != primero)
                    combinados.AddRange(t.Result.resultados);
            }

            var fusion = combinados.GroupBy(r => r.Pelicula.Id)
                            .Select(g => new Recomendacion{ Pelicula = g.First().Pelicula, Puntuacion = g.Max(x=>x.Puntuacion) })
                            .OrderByDescending(r => r.Puntuacion)
                            .Take(k)
                            .ToList();

            var tiempos = mapaSw.ToDictionary(kv => kv.Key, kv => kv.Value.Elapsed);
            return (fusion, tiempos);
        }

        private async Task<(List<Recomendacion> resultados, string nombre)> EjecutarMedido(IRecomendador estrategia, PerfilUsuario usuario, int k, CancellationToken ct, Dictionary<string,Stopwatch> mapa)
        {
            var sw = Stopwatch.StartNew();
            mapa[estrategia.Nombre] = sw;
            try
            {
                var r = await estrategia.RecomendarAsync(usuario, _corpus, k, ct);
                return (r, estrategia.Nombre);
            }
            finally { sw.Stop(); }
        }

        public async Task<List<Recomendacion>> CompararyMedirRendimientoAsync(PerfilUsuario usuario, int k)
        {
            return await MetricasRendimiento.CompararRendimientoAsync(
                Environment.ProcessorCount,
                async () => await EjecutarSecuencialAsync(usuario, k),
                async () => await EjecutarParaleloAsync(usuario, k)
            );
        }

        private async Task<List<Recomendacion>> EjecutarSecuencialAsync(PerfilUsuario usuario, int k)
        {
            var resultados = new List<Recomendacion>();
            foreach (var estrategia in _estrategias)
            {
                try
                {
                    var recomendaciones = await estrategia.RecomendarAsync(usuario, _corpus, k, CancellationToken.None);
                    resultados.AddRange(recomendaciones);
                }
                catch { }
            }

            return resultados.GroupBy(r => r.Pelicula.Id)
                            .Select(g => new Recomendacion{ Pelicula = g.First().Pelicula, Puntuacion = g.Max(x=>x.Puntuacion) })
                            .OrderByDescending(r => r.Puntuacion)
                            .Take(k)
                            .ToList();
        }

        private async Task<List<Recomendacion>> EjecutarParaleloAsync(PerfilUsuario usuario, int k)
        {
            var tareas = _estrategias.Select(s => Task.Run(async () => {
                try
                {
                    return await s.RecomendarAsync(usuario, _corpus, k, CancellationToken.None);
                }
                catch
                {
                    return new List<Recomendacion>();
                }
            }));

            var resultadosArray = await Task.WhenAll(tareas);
            var combinados = resultadosArray.SelectMany(x => x).ToList();

            return combinados.GroupBy(r => r.Pelicula.Id)
                            .Select(g => new Recomendacion{ Pelicula = g.First().Pelicula, Puntuacion = g.Max(x=>x.Puntuacion) })
                            .OrderByDescending(r => r.Puntuacion)
                            .Take(k)
                            .ToList();
        }
    }
}
