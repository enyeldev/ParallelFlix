using Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase {
    private readonly MetricsService _metrics;
    public MetricsController(MetricsService metrics) => _metrics = metrics;

    [HttpGet("snapshot")]
    public ActionResult<object> Snapshot() {
        var (avg, p95, rps) = _metrics.Snapshot();
        return Ok(new { avgMs = avg, p95Ms = p95, approxThroughput = rps });
    }
}
