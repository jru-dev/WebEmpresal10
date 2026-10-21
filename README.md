# Hotel.Web – EF Core Database First (Semana 10)

Aplicación ASP.NET Core MVC (.NET 8) de habitaciones y reservas de hotel con Entity Framework Core **Database First**, sin migraciones ni procedimientos almacenados.

## Cómo ejecutarlo
1. En SQL Server ejecutar `Database/01_HotelDB.sql` y luego `Database/02_Observaciones.sql` (agrega la columna `Observaciones`).
2. Ajustar la cadena de conexión `HotelDB` en `Hotel.Web/appsettings.json`.
3. `cd Hotel.Web` y `dotnet run` (o abrir `Hotel.sln` en Visual Studio).

## Comando de scaffold
Visual Studio (Consola del Administrador de paquetes):

    Scaffold-DbContext "Server=localhost;Database=HotelDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context HotelContext -Force

VS Code:

    dotnet ef dbcontext scaffold "Server=localhost;Database=HotelDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --force

> Tras regenerar, quitar el método `OnConfiguring` del contexto (la conexión se registra con `AddDbContext` en `Program.cs`).

## Estructura
- `Models/` – entidades y `HotelContext` generados por scaffold (no se editan).
- `Repositorios/` – `HabitacionRepositorio` y `ReservaRepositorio` (todas las consultas LINQ y `SaveChangesAsync`).
- `Controllers/` – `HabitacionesController` y `ReservasController` (solo usan repositorios).
- `ViewModels/` – clases de apoyo para los listados con totales.

