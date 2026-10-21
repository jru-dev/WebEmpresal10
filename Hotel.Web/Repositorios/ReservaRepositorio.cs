using Hotel.Web.Models;
using Hotel.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    // Habitaciones activas libres en el rango (una sola consulta con !Any)
    public async Task<List<HabitacionDisponibleVM>> DisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        int noches = salida.DayNumber - ingreso.DayNumber;

        return await _context.Habitacions
            .AsNoTracking()
            .Where(h => h.Activo == true
                && !h.Reservas.Any(r => r.Estado != "Cancelada"
                    && r.FechaIngreso < salida
                    && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .Select(h => new HabitacionDisponibleVM
            {
                IdHabitacion = h.IdHabitacion,
                Numero = h.Numero,
                Piso = h.Piso,
                Tipo = h.IdTipoHabitacionNavigation.Nombre,
                Capacidad = h.IdTipoHabitacionNavigation.Capacidad,
                PrecioNoche = h.IdTipoHabitacionNavigation.PrecioNoche,
                Noches = noches,
                Total = h.IdTipoHabitacionNavigation.PrecioNoche * noches
            })
            .ToListAsync();
    }

    // ¿La habitación ya está reservada en ese rango?
    public async Task<bool> HayCruceAsync(int idHabitacion, DateOnly ingreso, DateOnly salida)
    {
        return await _context.Reservas.AnyAsync(r => r.IdHabitacion == idHabitacion
            && r.Estado != "Cancelada"
            && r.FechaIngreso < salida
            && r.FechaSalida > ingreso);
    }

    public async Task<List<ReservaListadoVM>> ListarAsync()
    {
        var reservas = await _context.Reservas
            .AsNoTracking()
            .OrderByDescending(r => r.FechaIngreso)
            .Select(r => new
            {
                r.IdReserva,
                Huesped = r.IdHuespedNavigation.Nombres + " " + r.IdHuespedNavigation.Apellidos,
                Habitacion = r.IdHabitacionNavigation.Numero,
                r.FechaIngreso,
                r.FechaSalida,
                r.Estado,
                r.Observaciones,
                Precio = r.IdHabitacionNavigation.IdTipoHabitacionNavigation.PrecioNoche
            })
            .ToListAsync();

        // Noches y total se calculan en memoria
        return reservas.Select(r =>
        {
            int noches = r.FechaSalida.DayNumber - r.FechaIngreso.DayNumber;
            return new ReservaListadoVM
            {
                IdReserva = r.IdReserva,
                Huesped = r.Huesped,
                Habitacion = r.Habitacion,
                FechaIngreso = r.FechaIngreso,
                FechaSalida = r.FechaSalida,
                Estado = r.Estado,
                Observaciones = r.Observaciones,
                Noches = noches,
                Total = r.Precio * noches
            };
        }).ToList();
    }

    // Datos de las listas desplegables
    public async Task<List<Huesped>> ListarHuespedesAsync()
    {
        return await _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo == true)
            .OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ListarHabitacionesActivasAsync()
    {
        return await _context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo == true)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    // Solo se cancelan las Pendiente o Confirmada
    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null) return false;
        if (reserva.Estado != "Pendiente" && reserva.Estado != "Confirmada") return false;

        reserva.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}
