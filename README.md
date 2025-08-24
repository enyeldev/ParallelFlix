# 🎬 ParallelFlix

Sistema de recomendación de películas con algoritmos paralelos y especulativos desarrollado en C# con .NET 9/8.

## ✨ Características

- **Algoritmo Especulativo**: Ejecuta múltiples estrategias de recomendación en paralelo y utiliza la respuesta más rápida
- **Múltiples Estrategias de Recomendación**:
  - **Colaborativo**: Basado en usuarios con preferencias similares
  - **Contenido**: Basado en características de películas vistas
  - **Tendencias**: Basado en popularidad y calificaciones
- **Base de Datos SQLite**: Persistencia de datos con Entity Framework Core
- **Métricas de Rendimiento**: Análisis detallado de speedup y eficiencia del paralelismo
- **Interfaz de Consola Interactiva**: UI temática con navegación intuitiva

## 🏗️ Arquitectura

### Componentes Principales

```
src/
├── Datos/               # Contexto de base de datos y migraciones
├── Modelos/            # Entidades del dominio (Pelicula, PerfilUsuario, Recomendacion)
├── Recomendadores/     # Estrategias de recomendación
├── Servicios/          # Motor de recomendación y servicios de aplicación
├── Nucleo/             # Algoritmos de similitud
└── Utilidades/         # Helpers y tema de consola
```

### Algoritmo Especulativo

El motor de recomendación implementa un enfoque especulativo:

1. **Descomposición**: Lanza todas las estrategias en paralelo
2. **Competencia**: Utiliza la primera respuesta disponible
3. **Fusión**: Combina resultados parciales para mejorar precisión
4. **Cancelación**: Cancela tareas lentas para optimizar recursos

## 🚀 Instalación y Uso

### Prerrequisitos

- .NET 9 SDK
- SQLite (incluido con Entity Framework)

### Configuración

1. **Clonar el repositorio**:
   ```bash
   git clone [<url-del-repositorio>](https://github.com/enyeldev/ParallelFlix/)
   cd parallelflix
   ```

2. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

3. **Crear la base de datos**:
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```

### Uso de la Aplicación

Al iniciar, el sistema presenta una interfaz interactiva con las siguientes opciones:

- **Navegación Principal**: Ver recomendaciones, acceder a Mi Lista, buscar películas
- **Detalles de Película**: Información completa, reproducción simulada, agregar a lista
- **Mi Lista**: Gestión personalizada de películas favoritas
- **Búsqueda**: Filtros por género y título

## 📊 Métricas de Rendimiento

El sistema incluye análisis automático de rendimiento que mide:

- **Speedup**: Mejora de velocidad del procesamiento paralelo vs secuencial
- **Eficiencia**: Utilización efectiva de los procesadores disponibles
- **Throughput**: Recomendaciones procesadas por segundo
- **Tiempos de Respuesta**: Por cada estrategia de recomendación

## 🎯 Algoritmos de Recomendación

### Filtrado Colaborativo
Identifica patrones en usuarios con gustos similares basándose en:
- Historial de películas vistas
- Géneros preferidos
- Calificaciones implícitas

### Filtrado por Contenido
Recomienda basándose en características de películas:
- Similitud de géneros y etiquetas (Jaccard)
- Preferencias explícitas del usuario
- Análisis de la última película vista

### Recomendaciones por Tendencias
Sugiere contenido popular considerando:
- Calificaciones globales
- Factor de novedad temporal
- Ruido controlado para diversidad

## 🔧 Configuración Técnica

### Base de Datos

El sistema utiliza SQLite con las siguientes entidades:

- **Peliculas**: Catálogo principal con metadatos
- **PerfilesUsuarios**: Preferencias y historial de usuarios  
- **Recomendaciones**: Cache de puntuaciones calculadas

### Paralelización

- **Task.WhenAny()** para competencia especulativa
- **Parallel.ForEach()** para procesamiento de candidatos
- **ConcurrentBag<>** para agregación thread-safe de resultados
- **CancellationToken** para control de timeouts

## 🏆 Rendimiento

En un sistema con 4 núcleos típicamente se observa (aproximadamente, depende entre dispositivos):
- **Speedup**: 2.5x - 3.2x
- **Eficiencia**: 65% - 80%
- **Tiempo de Respuesta**: < 200ms para 10 recomendaciones
