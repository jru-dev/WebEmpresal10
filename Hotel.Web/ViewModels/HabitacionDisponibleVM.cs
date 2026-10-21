namespace Hotel.Web.ViewModels;

public class HabitacionDisponibleVM
{
    public int IdHabitacion { get; set; }
    public string Numero { get; set; } = "";
    public int Piso { get; set; }
    public string Tipo { get; set; } = "";
    public int Capacidad { get; set; }
    public decimal PrecioNoche { get; set; }
    public int Noches { get; set; }
    public decimal Total { get; set; }
}
