using API.Data;
using API.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public record ProductoResponse(int Id, string Nombre, decimal PrecioUnitario);

[ApiController]
[Route("api/productos")]
[Authorize(Policy = "Admin")]
public class ProductosController : ControllerBase
{
    private readonly DeliverySacContext _context;

    public ProductosController(DeliverySacContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductoResponse>>> Listar(
        [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1 || size > 100)
            return BadRequest("page debe ser >= 1 y size entre 1 y 100");

        var total = await _context.Productos.CountAsync();
        var items = await _context.Productos
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductoResponse(p.Id, p.Nombre, p.PrecioUnitario))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(total / (double)size);
        return Ok(new PagedResult<ProductoResponse>(items, page, size, total, totalPages));
    }
}
