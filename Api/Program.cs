using Api.Data;
using Api.Domain.Services;
using Api.Domain.Servicios.Algoritmos;
using Api.Doain.Servicios.Paralelismo;
using Microsoft.EntityFrameworkCore;

var generador = WebApplication.CreateBuilder(args);

generador.Services.AddDbContext<ContextoAplicacion>(opciones =>
    opciones.UseSqlite("Data Source=recomendaciones_netflix.db"));

generador.Services.AddMemoryCache();

generador.Services.AddControllers();

generador.Services.AddEndpointsApiExplorer();

generador.Services.AddSwaggerGen();


generador.Services.AddSingleton<CacheMatrizSimilitud>();
generador.Services.AddSingleton<BarrerasPipeline>();
generador.Services.AddScoped<ServicioFiltradoColaborativo>();
generador.Services.AddScoped<ServicioBasadoContenido>();
generador.Services.AddScoped<MezcladorHibrido>();
generador.Services.AddScoped<ServicioImpulsoTendencias>();
generador.Services.AddScoped<ServicioMetricas>();
generador.Services.AddScoped<MotorEspeculativoRecomendaciones>();

var aplicacion = generador.Build();

using (var alcance = aplicacion.Services.CreateScope()) {
    var db = alcance.ServiceProvider.GetRequiredService<ContextoAplicacion>();
    await db.Database.MigrateAsync();
    await DatosSemilla.AsegurarSemillaAsync(db);
}

aplicacion.UseSwagger();
aplicacion.UseSwaggerUI();

aplicacion.MapControllers();

aplicacion.Run();
