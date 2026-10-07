using API.Data;
using API.Domain;
using API.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public record CrearLineaRequest(int ProductoId, int Cantidad, decimal PrecioUnitario, string Descripcion);
public record CrearPedidoRequest(int ClienteId, DateTime FechaReparto, string? Comentario, List<CrearLineaRequest> Lineas);
public record LineaResponse(int Id, int ProductoId, string Producto, int Cantidad, decimal PrecioUnitario, string Descripcion);
public record PedidoResponse(int Id, int ClienteId, DateTime FechaReparto, string? Comentario, decimal Total, List<LineaResponse> Lineas);

[ApiController]
[Route("api/pedidos")]
[Authorize(Policy = "Admin")]
public class PedidosController : ControllerBase
{
    private readonly DeliverySacContext _context;

    public PedidosController(DeliverySacContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Crear(CrearPedidoRequest request)
    {
        var errores = new List<string>();

        if (request.Lineas is null || request.Lineas.Count == 0)
            errores.Add("El pedido debe tener al menos una linea");

        if (!await _context.Clientes.AnyAsync(c => c.Id == request.ClienteId))
            errores.Add("El cliente indicado no existe");

        var productoIds = request.Lineas?.Select(l => l.ProductoId).Distinct().ToList() ?? new();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var (linea, index) in (request.Lineas ?? new()).Select((l, i) => (l, i)))
        {
            if (!productos.ContainsKey(linea.ProductoId))
                errores.Add($"Linea {index + 1}: producto inexistente");
            if (linea.Cantidad <= 0)
                errores.Add($"Linea {index + 1}: cantidad debe ser mayor a 0");
            if (linea.PrecioUnitario < 0)
                errores.Add($"Linea {index + 1}: precio unitario no puede ser negativo");
            if (string.IsNullOrWhiteSpace(linea.Descripcion))
                errores.Add($"Linea {index + 1}: descripcion obligatoria");
        }

        if (errores.Count > 0)
            return BadRequest(errores);

        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            FechaReparto = request.FechaReparto,
            Comentario = request.Comentario,
            Total = request.Lineas!.Sum(l => l.Cantidad * l.PrecioUnitario)
        };

        foreach (var l in request.Lineas!)
        {
            pedido.Lineas.Add(new LineaPedido
            {
                ProductoId = l.ProductoId,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                Descripcion = l.Descripcion.Trim()
            });
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = pedido.Id }, ToResponse(pedido));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PedidoResponse>> Obtener(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound();

        return ToResponse(pedido);
    }

    private static PedidoResponse ToResponse(Pedido pedido) => new(
        pedido.Id,
        pedido.ClienteId,
        pedido.FechaReparto,
        pedido.Comentario,
        pedido.Total,
        pedido.Lineas.Select(l => new LineaResponse(l.Id, l.ProductoId, string.Empty, l.Cantidad, l.PrecioUnitario, l.Descripcion)).ToList());
}
