using API.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;

namespace API.Controllers;

public record CrearClienteRequest(string Nombre, string Direccion);
public record ClienteResponse(int Id, string Nombre, string Direccion);

[ApiController]
[Route("api/clientes")]
[Authorize(Policy = "Admin")]
public class ClientesController : ControllerBase
{
    private readonly DeliverySacContext _context;

    public ClientesController(DeliverySacContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Crear(CrearClienteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Direccion))
            return BadRequest("Nombre y direccion son obligatorios");

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Direccion = request.Direccion.Trim()
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = cliente.Id },
            new ClienteResponse(cliente.Id, cliente.Nombre, cliente.Direccion));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteResponse>> Obtener(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound();

        return new ClienteResponse(cliente.Id, cliente.Nombre, cliente.Direccion);
    }
}
