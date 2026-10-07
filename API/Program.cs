using System.Text;
using API.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<API.Data.DeliverySacContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DeliverySac")));

var jwtOptions = builder.Configuration.GetSection(API.Services.JwtOptions.Section).Get<API.Services.JwtOptions>()
    ?? throw new InvalidOperationException("Falta la seccion Jwt en la configuracion");
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<API.Services.JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole(nameof(Rol.ADMIN)))
    .AddPolicy("Repartidor", p => p.RequireRole(nameof(Rol.REPARTIDOR)))
    .AddPolicy("Supervisor", p => p.RequireRole(nameof(Rol.SUPERVISOR)));

builder.Services.AddControllers();
builder.Services.AddSingleton<API.Services.IPasswordHasher, API.Services.BCryptPasswordHasher>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<API.Data.DeliverySacContext>();
    API.Data.DbInitializer.Seed(context, scope.ServiceProvider.GetRequiredService<API.Services.IPasswordHasher>());
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
