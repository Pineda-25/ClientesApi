using ClientesApi.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

//  Construir la apliacion (motor)

var builder = WebApplication.CreateBuilder(args);

// Registrar el DbContext
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//otros servicios
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
