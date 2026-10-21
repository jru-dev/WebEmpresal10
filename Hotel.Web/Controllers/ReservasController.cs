using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _repo;

    public ReservasController(ReservaRepositorio repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _repo.ListarAsync());
    }

    // Filtro por GET: ?ingreso=...&salida=...
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;

        if (!ingreso.HasValue || !salida.HasValue)
            return View(model: null);

        if (salida <= ingreso)
        {
            ViewBag.Error = "La fecha de salida debe ser posterior a la de ingreso.";
            return View(model: null);
        }

        return View(await _repo.DisponiblesAsync(ingreso.Value, salida.Value));
    }

    private async Task CargarListasAsync(Reserva? reserva = null)
    {
        var huespedes = (await _repo.ListarHuespedesAsync())
            .Select(h => new { h.IdHuesped, Texto = $"{h.Apellidos}, {h.Nombres} ({h.Dni})" });
        var habitaciones = (await _repo.ListarHabitacionesActivasAsync())
            .Select(h => new { h.IdHabitacion, Texto = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre}" });

        ViewBag.Huespedes = new SelectList(huespedes, "IdHuesped", "Texto", reserva?.IdHuesped);
        ViewBag.Habitaciones = new SelectList(habitaciones, "IdHabitacion", "Texto", reserva?.IdHabitacion);
    }

    public async Task<IActionResult> Create(int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var reserva = new Reserva
        {
            IdHabitacion = idHabitacion ?? 0,
            FechaIngreso = ingreso ?? hoy,
            FechaSalida = salida ?? hoy.AddDays(1)
        };

        await CargarListasAsync(reserva);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida,Observaciones")] Reserva reserva)
    {
        // Estado lo asigna el servidor; las propiedades de navegación no vienen del formulario
        ModelState.Remove(nameof(Reserva.Estado));
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        if (reserva.FechaSalida <= reserva.FechaIngreso)
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La salida debe ser posterior al ingreso.");

        if (reserva.FechaIngreso < hoy)
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "El ingreso no puede ser anterior a hoy.");

        if (ModelState.IsValid
            && await _repo.HayCruceAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango de fechas.");
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(reserva);
            return View(reserva);
        }

        await _repo.CrearAsync(reserva);
        TempData["Mensaje"] = "Reserva registrada como Pendiente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (await _repo.CancelarAsync(id))
            TempData["Mensaje"] = "Reserva cancelada.";
        else
            TempData["Error"] = "Solo se pueden cancelar reservas Pendiente o Confirmada.";

        return RedirectToAction(nameof(Index));
    }
}
