using Xunit;
using src.Servicios;
using System;
using System.Threading.Tasks;

public class MetricasTests
{
    [Fact]
    public void Medir_AccionSincrona_AlmacenaTiempo()
    {
        var metricas = new Metricas();

        metricas.Medir("accion1", () =>
        {
            // Simula trabajo
            Task.Delay(100).Wait();
        });

        Assert.True(metricas.TiemposPorEstrategia.ContainsKey("accion1"));
        Assert.True(metricas.TiemposPorEstrategia["accion1"].TotalMilliseconds >= 100);
    }

    [Fact]
    public async Task MedirAsync_AccionAsincrona_AlmacenaTiempo()
    {
        var metricas = new Metricas();

        var resultado = await metricas.MedirAsync("accionAsync", async () =>
        {
            await Task.Delay(50);
            return 42;
        });

        Assert.Equal(42, resultado);
        Assert.True(metricas.TiemposPorEstrategia.ContainsKey("accionAsync"));
        Assert.True(metricas.TiemposPorEstrategia["accionAsync"].TotalMilliseconds >= 50);
    }

    [Fact]
    public void FinalizarMetricas_CalculaTiempoTotalYConteo()
    {
        var metricas = new Metricas();

        metricas.Medir("a", () => Task.Delay(10).Wait());
        metricas.Medir("b", () => Task.Delay(20).Wait());

        metricas.FinalizarMetricas(5);

        Assert.Equal(5, metricas.ConteoResultados);
        Assert.True(metricas.TiempoTotal.TotalMilliseconds >= 30);
    }

    [Fact]
    public void ThroughputPorSegundo_CalculaCorrectamente()
    {
        var metricas = new Metricas();

        metricas.Medir("a", () => Task.Delay(50).Wait());
        metricas.FinalizarMetricas(100);

        var throughput = metricas.ThroughputPorSegundo;
        Assert.True(throughput > 0);
        Assert.InRange(throughput, 1, 10000); // sanity check
    }
}
