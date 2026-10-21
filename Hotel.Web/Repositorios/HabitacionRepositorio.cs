using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Habitaciones activas con su tipo, filtro opcional por tipo y orden por número
    public async Task<List<Habitacion>> ListarActivasAsync(int? idTipo)
    {
        var query = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo == true);

        if (idTipo.HasValue)
            query = query.Where(h => h.IdTipoHabitacion == idTipo.Value);

        return await query.OrderBy(h => h.Numero).ToListAsync();
    }

    // Datos de la lista desplegable de tipos
    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return await _context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo == true)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<Habitacion?> ObtenerAsync(int id)
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo == true);
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int? excluirId = null)
    {
        return await _context.Habitacions
            .AnyAsync(h => h.Numero == numero && h.IdHabitacion != excluirId);
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.Activo = true;
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Habitacion datos)
    {
        var habitacion = await _context.Habitacions.FindAsync(datos.IdHabitacion);
        if (habitacion == null || habitacion.Activo != true) return false;

        habitacion.Numero = datos.Numero;
        habitacion.Piso = datos.Piso;
        habitacion.IdTipoHabitacion = datos.IdTipoHabitacion;
        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminación lógica: Activo = 0
    public async Task<bool> DesactivarAsync(int id)
    {
        var habitacion = await _context.Habitacions.FindAsync(id);
        if (habitacion == null || habitacion.Activo != true) return false;

        habitacion.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
