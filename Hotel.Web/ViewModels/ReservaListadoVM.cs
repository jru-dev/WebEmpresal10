namespace Hotel.Web.ViewModels;

public class ReservaListadoVM
{
    public int IdReserva { get; set; }
    public string Huesped { get; set; } = "";
    public string Habitacion { get; set; } = "";
    public DateOnly FechaIngreso { get; set; }
    public DateOnly FechaSalida { get; set; }
    public string Estado { get; set; } = "";
    public string? Observaciones { get; set; }
    public int Noches { get; set; }
    public decimal Total { get; set; }
    public bool PuedeCancelar => Estado == "Pendiente" || Estado == "Confirmada";
}
