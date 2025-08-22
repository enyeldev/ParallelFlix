using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;
using System.Text.Json;
using System.Diagnostics;


namespace src.Servicios
{
    public sealed class Metricas
    {
        public TimeSpan TiempoTotal { get; private set; }
        public Dictionary<string, TimeSpan> TiemposPorEstrategia { get; } = new();
        public int ConteoResultados { get; private set; }

        public void Medir(string clave, Action accion)
        {
            var sw = Stopwatch.StartNew();
            accion();
            sw.Stop();
            TiemposPorEstrategia[clave] = sw.Elapsed;
        }

        public async Task<T> MedirAsync<T>(string clave, Func<Task<T>> fabrica)
        {
            var sw = Stopwatch.StartNew();
            var resultado = await fabrica();
            sw.Stop();
            TiemposPorEstrategia[clave] = sw.Elapsed;
            return resultado;
        }

        public void FinalizarMetricas(int resultados)
        {
            ConteoResultados = resultados;
            TiempoTotal = TiemposPorEstrategia.Values.Aggregate(TimeSpan.Zero, (a,b)=>a+b);
        }

        public double ThroughputPorSegundo => TiempoTotal.TotalSeconds == 0 ? 0 : ConteoResultados / TiempoTotal.TotalSeconds;
    }
}