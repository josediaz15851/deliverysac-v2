namespace API.Domain;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime FechaReparto { get; set; }
    public string? Comentario { get; set; }
    public decimal Total { get; set; }
    public ICollection<LineaPedido> Lineas { get; set; } = new List<LineaPedido>();
}
