using API.Domain;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DeliverySacContext : DbContext
{
    public DeliverySacContext(DbContextOptions<DeliverySacContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<LineaPedido> LineasPedido => Set<LineaPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.UsuarioNombre)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        modelBuilder.Entity<Usuario>()
            .Property(u => u.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<Cliente>()
            .Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Cliente>()
            .Property(c => c.Direccion)
            .IsRequired()
            .HasMaxLength(300);

        modelBuilder.Entity<Producto>()
            .Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioUnitario)
            .HasPrecision(12, 2);

        modelBuilder.Entity<Pedido>()
            .HasOne(d => d.Cliente)
            .WithMany()
            .HasForeignKey(d => d.ClienteId);

        modelBuilder.Entity<Pedido>()
            .Property(d => d.Total)
            .HasPrecision(12, 2);

        modelBuilder.Entity<LineaPedido>()
            .HasOne(l => l.Pedido)
            .WithMany(d => d.Lineas)
            .HasForeignKey(l => l.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LineaPedido>()
            .HasOne(l => l.Producto)
            .WithMany()
            .HasForeignKey(l => l.ProductoId);

        modelBuilder.Entity<LineaPedido>()
            .Property(l => l.PrecioUnitario)
            .HasPrecision(12, 2);

        modelBuilder.Entity<LineaPedido>()
            .Property(l => l.Descripcion)
            .IsRequired();
    }
}
