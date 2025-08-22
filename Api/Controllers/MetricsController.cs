using Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricasController : ControllerBase {
    private readonly ServicioMetricas _metricas;
    public MetricasController(ServicioMetricas metricas) => _metricas = metricas;

    [HttpGet("instantanea")]
    public ActionResult<object> Instantanea() {
        var (promedio, p95, muestras) = _metricas.Instantanea();
        return Ok(new { promedioMs = promedio, p95Ms = p95, throughputAprox = muestras });
    }
}

