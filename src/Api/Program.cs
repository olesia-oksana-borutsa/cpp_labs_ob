using Api.Endpoints; 
using Scalar.AspNetCore;
using Core.Abstractions;
using Core.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); 
builder.Services.AddSingleton<ICatalogStore, InMemoryCatalogStore>(); 

var app = builder.Build();

app.UseExceptionHandler(); 
app.UseStatusCodePages();

if (app.Environment.IsDevelopment()) 
{ 
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();