using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using src.Modelos;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;

namespace src.Servicios
{
    public sealed class Metricas
    {
        public TimeSpan TiempoTotal { get; private set; }
        public Dictionary<string, TimeSpan> TiemposPorEstrategia { get; } = new();
        public int ConteoResultados { get; private set; }
        
        public TimeSpan TiempoSecuencial { get; private set; }
        public TimeSpan TiempoParalelo { get; private set; }
        public int NumProcesadores { get; private set; }
        public double Speedup => TiempoSecuencial.TotalMilliseconds / (TiempoParalelo.TotalMilliseconds == 0 ? 1 : TiempoParalelo.TotalMilliseconds);
        public double Eficiencia => NumProcesadores == 0 ? 0 : Speedup / NumProcesadores;

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

        public T CompararRendimiento<T>(int procesadoresDisponibles, Func<T> operacionSecuencial, Func<T> operacionParalela)
        {
            NumProcesadores = procesadoresDisponibles;
            
            var swSecuencial = Stopwatch.StartNew();
            var resultadoSecuencial = operacionSecuencial();
            swSecuencial.Stop();
            TiempoSecuencial = swSecuencial.Elapsed;
            
            var swParalelo = Stopwatch.StartNew();
            var resultadoParalelo = operacionParalela();
            swParalelo.Stop();
            TiempoParalelo = swParalelo.Elapsed;
            
            TiemposPorEstrategia["Secuencial"] = TiempoSecuencial;
            TiemposPorEstrategia["Paralelo"] = TiempoParalelo;
            
            return resultadoParalelo;
        }
        
        public async Task<T> CompararRendimientoAsync<T>(int procesadoresDisponibles, Func<Task<T>> operacionSecuencial, Func<Task<T>> operacionParalela)
        {
            NumProcesadores = procesadoresDisponibles;
            
            var swSecuencial = Stopwatch.StartNew();
            var resultadoSecuencial = await operacionSecuencial();
            swSecuencial.Stop();
            TiempoSecuencial = swSecuencial.Elapsed;
            
            var swParalelo = Stopwatch.StartNew();
            var resultadoParalelo = await operacionParalela();
            swParalelo.Stop();
            TiempoParalelo = swParalelo.Elapsed;
            
            TiemposPorEstrategia["Secuencial"] = TiempoSecuencial;
            TiemposPorEstrategia["Paralelo"] = TiempoParalelo;
            
            return resultadoParalelo;
        }
        
        public void MostrarMetricasParalelismo()
        {
            Console.WriteLine($"---- Metricas de Paralelismo con {NumProcesadores} procesadores ----");
            Console.WriteLine($"Tiempo secuencial: {TiempoSecuencial.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Tiempo paralelo: {TiempoParalelo.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Speedup: {Speedup:F2}x");
            Console.WriteLine($"Eficiencia: {Eficiencia:P2}");
            Console.WriteLine();
        }
        
        public string ObtenerResumenMetricas()
        {
            return $"Procesadores: {NumProcesadores}, " +
                   $"T. Secuencial: {TiempoSecuencial.TotalMilliseconds:F2} ms, " +
                   $"T. Paralelo: {TiempoParalelo.TotalMilliseconds:F2} ms, " +
                   $"Speedup: {Speedup:F2}x, " +
                   $"Eficiencia: {Eficiencia:P2}, " +
                   $"Resultados: {ConteoResultados}, " +
                   $"Throughput: {ThroughputPorSegundo:F2} resultados/s";
        }
    }
}
