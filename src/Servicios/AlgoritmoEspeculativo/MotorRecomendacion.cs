using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;
using src.Recomendadores;
using System.Diagnostics;

namespace src.Servicios.AlgoritmoEspeculativo
{
    public class MotorRecomendacion
    {
        private readonly List<Pelicula> _corpus;
        private readonly List<IRecomendador> _estrategias;

        public MotorRecomendacion(List<Pelicula> corpus)
        {
            _corpus = corpus;
            _estrategias = new() { new RecomendadorContenido(), new RecomendadorColaborativo(), new RecomendadorTendencias() };
        }

        // Descomposición especulativa: lanzar todas las estrategias y tomar la que responde primero, con fusión sencilla.
        public async Task<(List<Recomendacion> resultados, Dictionary<string, TimeSpan> tiempos)> RecomendarEspeculativoAsync(PerfilUsuario usuario, int k)
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
                            .Select(g => new Recomendacion { Pelicula = g.First().Pelicula, Puntuacion = g.Max(x => x.Puntuacion) })
                            .OrderByDescending(r => r.Puntuacion)
                            .Take(k)
                            .ToList();

            var tiempos = mapaSw.ToDictionary(kv => kv.Key, kv => kv.Value.Elapsed);
            return (fusion, tiempos);
        }

        private async Task<(List<Recomendacion> resultados, string nombre)> EjecutarMedido(IRecomendador estrategia, PerfilUsuario usuario, int k, CancellationToken ct, Dictionary<string, Stopwatch> mapa)
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
    }
}