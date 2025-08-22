using Api.Data;
using Api.Domain.DTOs;
using Api.Domain.Services.Algoritmos;
using Api.Domain.Services.EjecucionEspeculativa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecomendacionesController : ControllerBase {
    private readonly MotorEspeculativoRecomendaciones _motor;
    private readonly ServicioFiltradoColaborativo _colab;
    private readonly ServicioBasadoContenido _cont;
    private readonly ContextoAplicacion _db;

    public RecomendacionesController(
        MotorEspeculativoRecomendaciones motor,
        ServicioFiltradoColaborativo colab,
        ServicioBasadoContenido cont,
        ContextoAplicacion db)
    { _motor = motor; _colab = colab; _cont = cont; _db = db; }

    [HttpGet("{usuarioId}")]
    public async Task<ActionResult<IEnumerable<RecomendacionDto>>> Obtener(
        int usuarioId, [FromQuery] int k = 20, [FromQuery] string modo = "especulativo",
        [FromQuery] int slaMs = 150, CancellationToken ct = default)
    {
        if (!await _db.Usuarios.AnyAsync(u => u.Id == usuarioId, ct))
            return NotFound($"Usuario {usuarioId} no encontrado");

        var sla = TimeSpan.FromMilliseconds(Math.Max(50, slaMs));
        var resultado = modo switch {
            "colaborativo" => await _colab.RecomendarAsync(usuarioId, k, ct),
            "contenido"    => await _cont.RecomendarAsync(usuarioId, k, ct),
            _               => await _motor.RecomendarAsync(usuarioId, k, sla, ct)
        };

        var payload = resultado.Items.Select(i =>
            new RecomendacionDto(i.PeliculaId, i.Titulo, i.Puntuacion, resultado.Algoritmo, (long)resultado.Duracion.TotalMilliseconds));
        return Ok(payload);
    }
}
