
using Microsoft.EntityFrameworkCore;
using src.Datos;


// Crear el factory (usa el que implementamos para migraciones y también en runtime)
var factory = new AppDbContextoFactory();
using var contexto = factory.CreateDbContext(args);

// Asegura que la base de datos y tablas existen
contexto.Database.EnsureCreated();

// var opciones = new DbContextOptionsBuilder<AppDbContext>()
//     .UseSqlite("Data Source=db.db")
//     .Options;


// using var contexto = new AppDbContext(opciones);


// contexto.Database.EnsureCreated();