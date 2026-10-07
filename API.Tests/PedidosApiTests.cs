using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Data;
using API.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests;

public class PedidosApiTests : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DeliverySacContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<DeliverySacContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    private async Task<(HttpClient Client, List<int> ProductoIds, int ClienteId)> PrepararAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeliverySacContext>();
        if (!context.Productos.Any())
        {
            context.Productos.AddRange(
                new Producto { Id = 1, Nombre = "Gaseosa 2L", PrecioUnitario = 2.50m },
                new Producto { Id = 2, Nombre = "Arroz 5kg", PrecioUnitario = 7.00m });
        }
        if (!context.Clientes.Any())
        {
            context.Clientes.Add(new Cliente { Id = 1, Nombre = "Cliente Demo", Direccion = "Dir Demo 1" });
        }
        await context.SaveChangesAsync();

        var client = CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { usuario = "admin", password = "Admin123!" });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginResult>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);

        return (client, new List<int> { 1, 2 }, 1);
    }

    private record LoginResult(string Token, string Rol);
    private record LineaResponse(int Id, int ProductoId, string Producto, int Cantidad, decimal PrecioUnitario, string Descripcion);
    private record PedidoResponse(int Id, int ClienteId, DateTime FechaReparto, string? Comentario, decimal Total, List<LineaResponse> Lineas);

    [Fact]
    public async Task CrearPedidoValido_TotalCalculadoPorBackend()
    {
        var (client, _, clienteId) = await PrepararAsync();

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = DateTime.Today,
            comentario = "pedido AC-04",
            lineas = new[]
            {
                new { productoId = 1, cantidad = 2, precioUnitario = 2.50m, descripcion = "Dos gaseosas" },
                new { productoId = 2, cantidad = 1, precioUnitario = 7.00m, descripcion = "Un arroz" }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        Assert.Equal(12.00m, pedido!.Total);
        Assert.Equal(2, pedido.Lineas.Count);

        var get = await client.GetFromJsonAsync<PedidoResponse>($"/api/pedidos/{pedido.Id}");
        Assert.Equal(12.00m, get!.Total);
    }

    [Fact]
    public async Task CrearPedidoSinLineas_Retorna400()
    {
        var (client, _, clienteId) = await PrepararAsync();

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = DateTime.Today,
            lineas = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0, 2.50, "cant cero")]
    [InlineData(-1, 2.50, "cant negativa")]
    [InlineData(2, -0.01, "precio negativo")]
    public async Task CrearPedidoLineaInvalida_Retorna400(int cantidad, double precio, string descripcion)
    {
        var (client, _, clienteId) = await PrepararAsync();

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = DateTime.Today,
            lineas = new[]
            {
                new { productoId = 1, cantidad, precioUnitario = (decimal)precio, descripcion }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedidoLineaSinDescripcion_Retorna400()
    {
        var (client, _, clienteId) = await PrepararAsync();

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = DateTime.Today,
            lineas = new[]
            {
                new { productoId = 1, cantidad = 2, precioUnitario = 2.50m, descripcion = "   " }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedidoComoRepartidor_Retorna403()
    {
        var (client, _, clienteId) = await PrepararAsync();

        var repLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { usuario = "repartidor", password = "Repartidor123!" });
        var repBody = await repLogin.Content.ReadFromJsonAsync<LoginResult>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", repBody!.Token);

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = DateTime.Today,
            lineas = new[]
            {
                new { productoId = 1, cantidad = 1, precioUnitario = 2.50m, descripcion = "un item" }
            }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
