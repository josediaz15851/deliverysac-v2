using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<API.Data.DeliverySacContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DeliverySac")));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
