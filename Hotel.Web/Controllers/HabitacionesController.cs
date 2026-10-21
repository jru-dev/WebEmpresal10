using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _repo;

    public HabitacionesController(HabitacionRepositorio repo)
    {
        _repo = repo;
    }

    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        var tipos = await _repo.ListarTiposAsync();
        ViewBag.Tipos = new SelectList(tipos, "IdTipoHabitacion", "Nombre", seleccionado);
    }

    public async Task<IActionResult> Index(int? idTipo)
    {
        await CargarTiposAsync(idTipo);
        return View(await _repo.ListarActivasAsync(idTipo));
    }

    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await _repo.ObtenerAsync(id);
        if (habitacion == null) return NotFound();
        return View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));

        if (ModelState.IsValid)
        {
            if (await _repo.ExisteNumeroAsync(habitacion.Numero))
                ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con ese número.");
            else if (habitacion.Numero.Length > 10)
                ModelState.AddModelError(nameof(Habitacion.Numero), "El número admite como máximo 10 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        await _repo.CrearAsync(habitacion);
        TempData["Mensaje"] = "Habitación registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await _repo.ObtenerAsync(id);
        if (habitacion == null) return NotFound();

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return NotFound();

        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));

        if (ModelState.IsValid)
        {
            if (await _repo.ExisteNumeroAsync(habitacion.Numero, habitacion.IdHabitacion))
                ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con ese número.");
            else if (habitacion.Numero.Length > 10)
                ModelState.AddModelError(nameof(Habitacion.Numero), "El número admite como máximo 10 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.IdTipoHabitacion);
            return View(habitacion);
        }

        if (!await _repo.ActualizarAsync(habitacion)) return NotFound();

        TempData["Mensaje"] = "Habitación actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await _repo.ObtenerAsync(id);
        if (habitacion == null) return NotFound();
        return View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _repo.DesactivarAsync(id)) return NotFound();

        TempData["Mensaje"] = "Habitación eliminada (baja lógica).";
        return RedirectToAction(nameof(Index));
    }
}
