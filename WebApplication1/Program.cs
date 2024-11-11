using Microsoft.EntityFrameworkCore;
using System;
using WebApplication1.Context;
using WebApplication1.Filters;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Configura��o da string de conex�o
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configura��o do DbContext com MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// Configura��o dos servi�os
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>(); // Adiciona o filtro globalmente
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
