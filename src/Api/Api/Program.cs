// Api/Program.cs
using Api.Data;
using Api.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

 builder.Services.AddDbContext<AppDbContext>(opt =>
     opt.UseSqlite("Data Source=netflix_recs.db")); // simple para demo

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// servicios y metricas
//builder.Services.AddSingleton<SimilarityMatrixCache>();
//builder.Services.AddSingleton<PipelineBarriers>();
builder.Services.AddScoped<CollaborativeFilteringService>();
builder.Services.AddScoped<ContentBasedService>();
//builder.Services.AddScoped<HybridBlender>();
builder.Services.AddScoped<TrendingBoostService>();
//builder.Services.AddScoped<MetricsService>();
//builder.Services.AddScoped<SpeculativeRecommendationEngine>();

var app = builder.Build();
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
