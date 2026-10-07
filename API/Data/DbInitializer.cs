using API.Domain;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public static class DbInitializer
{
    public static void Seed(DbContext context, IPasswordHasher hasher)
    {
        if (context.Database.IsRelational())
            context.Database.Migrate();

        if (!context.Set<Producto>().Any())
        {
            context.Set<Producto>().AddRange(
                new Producto { Nombre = "Gaseosa 2L", PrecioUnitario = 2.50m },
                new Producto { Nombre = "Arroz 5kg", PrecioUnitario = 7.00m });
        }

        if (context.Set<Usuario>().Any())
        {
            context.SaveChanges();
            return;
        }

        var usuarios = new[]
        {
            new Usuario { Nombre = "Administrador", UsuarioNombre = "admin", PasswordHash = hasher.Hash("Admin123!"), Rol = Rol.ADMIN },
            new Usuario { Nombre = "Repartidor Demo", UsuarioNombre = "repartidor", PasswordHash = hasher.Hash("Repartidor123!"), Rol = Rol.REPARTIDOR },
            new Usuario { Nombre = "Supervisor Demo", UsuarioNombre = "supervisor", PasswordHash = hasher.Hash("Supervisor123!"), Rol = Rol.SUPERVISOR }
        };

        context.Set<Usuario>().AddRange(usuarios);
        context.SaveChanges();
    }
}
