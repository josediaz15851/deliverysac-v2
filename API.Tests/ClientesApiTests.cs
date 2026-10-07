using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests;

public class ClientesApiTests : WebApplicationFactory<Program>
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

    private async Task<HttpClient> ClienteAutenticadoAsync(string usuario, string password)
    {
        var client = CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { usuario, password });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginResult>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    private record LoginResult(string Token, string Rol);
    private record ClienteResponse(int Id, string Nombre, string Direccion);
    private record PagedResult<T>(System.Collections.Generic.IReadOnlyList<T> Items, int Page, int Size, int Total, int TotalPages);

    [Fact]
    public async Task ListarClientes_Paginado_Respetaloslimites()
    {
        var client = await ClienteAutenticadoAsync("admin", "Admin123!");

        for (var i = 1; i <= 12; i++)
        {
            var create = await client.PostAsJsonAsync("/api/clientes",
                new { nombre = $"Cliente {i}", direccion = $"Dir {i}" });
            create.EnsureSuccessStatusCode();
        }

        var page1 = await client.GetFromJsonAsync<PagedResult<ClienteResponse>>("/api/clientes?page=1&size=5");
        var page2 = await client.GetFromJsonAsync<PagedResult<ClienteResponse>>("/api/clientes?page=2&size=5");
        var page3 = await client.GetFromJsonAsync<PagedResult<ClienteResponse>>("/api/clientes?page=3&size=5");

        Assert.Equal(5, page1!.Items.Count);
        Assert.Equal(5, page2!.Items.Count);
        Assert.Equal(2, page3!.Items.Count);
        Assert.Equal(12, page1.Total);
        Assert.Equal(3, page1.TotalPages);
        Assert.Equal(1, page1.Page);
        Assert.NotEqual(page1.Items[0].Id, page2.Items[0].Id);
    }

    [Fact]
    public async Task ListarClientes_ParametrosInvalidos_Retorna400()
    {
        var client = await ClienteAutenticadoAsync("admin", "Admin123!");

        var response = await client.GetAsync("/api/clientes?page=0&size=5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearClienteComoAdmin_Retorna201YRecuperablePorId()
    {
        var client = await ClienteAutenticadoAsync("admin", "Admin123!");

        var response = await client.PostAsJsonAsync("/api/clientes",
            new { nombre = "Cliente AC-02", direccion = "Av. Siempre Verde 123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var creado = await response.Content.ReadFromJsonAsync<ClienteResponse>();

        var get = await client.GetAsync($"/api/clientes/{creado!.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var recuperado = await get.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.Equal("Cliente AC-02", recuperado!.Nombre);
    }

    [Theory]
    [InlineData("", "Av. Siempre Verde 123")]
    [InlineData("Cliente", "")]
    [InlineData("   ", "   ")]
    public async Task CrearCliente_SinNombreODireccion_Retorna400(string nombre, string direccion)
    {
        var client = await ClienteAutenticadoAsync("admin", "Admin123!");

        var response = await client.PostAsJsonAsync("/api/clientes",
            new { nombre, direccion });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearCliente_Comoadmin_SinToken_Retorna401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/clientes",
            new { nombre = "X", direccion = "Y" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CrearClienteComoRepartidor_Retorna403()
    {
        var client = await ClienteAutenticadoAsync("repartidor", "Repartidor123!");

        var response = await client.PostAsJsonAsync("/api/clientes",
            new { nombre = "X", direccion = "Y" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListarClientesComoRepartidor_Retorna403()
    {
        var client = await ClienteAutenticadoAsync("repartidor", "Repartidor123!");

        var response = await client.GetAsync("/api/clientes?page=1&size=10");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
