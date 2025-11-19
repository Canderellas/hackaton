using Backend_hackaton.Models;
using Backend_hackaton.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDeviceService, DeviceService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

try
{
    using var connection = new Npgsql.NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Database connection successful");
    
    // Проверим доступность таблиц
    using var cmd = new Npgsql.NpgsqlCommand("SELECT COUNT(*) FROM your_table_name", connection);
    var result = await cmd.ExecuteScalarAsync();
    Console.WriteLine($"✅ Table check successful, records count: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    // На время разработки можно использовать заглушку
    // throw; // раскомментируйте в продакшене
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://comunada.store")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
