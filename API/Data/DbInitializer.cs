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

        if (context.Set<Usuario>().Any())
            return;

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
