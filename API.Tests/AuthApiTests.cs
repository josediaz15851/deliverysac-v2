using System.Net;
using System.Net.Http.Json;
using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests;

public class AuthApiTests : WebApplicationFactory<Program>
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

    private async Task<string> LoginAsync(string usuario, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { usuario, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResult>();
        return body!.Token;
    }

    private record LoginResult(string Token, string Rol);

    [Fact]
    public async Task Login_ConCredencialesValidas_RetornaTokenYRol()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { usuario = "admin", password = "Admin123!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResult>();
        Assert.False(string.IsNullOrEmpty(body!.Token));
        Assert.Equal("ADMIN", body.Rol);
    }

    [Theory]
    [InlineData("admin", "PasswordIncorrecta!")]
    [InlineData("usuario-inexistente", "Admin123!")]
    public async Task Login_ConCredencialesInvalidas_Retorna401(string usuario, string password)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { usuario, password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Perfil_SinToken_Retorna401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/auth/perfil");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Perfil_ConTokenDeAdmin_RetornaRolAdmin()
    {
        var client = CreateClient();
        var token = await LoginAsync("admin", "Admin123!");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/auth/perfil");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PerfilResult>();
        Assert.Equal("ADMIN", body!.Rol);
    }

    private record PerfilResult(string Usuario, string Rol);
}
