using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    
    options.Limits.MaxRequestBufferSize = 16384; // 16KB - Suficiente para la línea + cabeceras pequeñas

   
    options.Limits.MaxRequestHeadersTotalSize = 1024; // 4KB. Esto es menor que 5KB (nuestra cabecera de ataque)
                                                     // y menor que 8KB (el límite de Varnish).

    options.Limits.MaxRequestHeaderCount = 20; // Reduce el número de cabeceras permitidas
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "data", "cachepoisoning.db")}"));

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(); // Para API simple si es necesario

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (!dbContext.Settings.Any())
        {
            dbContext.Settings.Add(new AppSetting { Key = "CachedMessage", Value = "Initial cached message from DB." });
            await dbContext.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMethodOverride();

app.UseAuthorization();

app.MapRazorPages();

app.Map("/resource", async context =>
{
    context.Response.Headers.Add("Cache-Control", "no-cache, no-store"); // Asegúrate de no cachear esto
    var originalMethod = context.Request.Headers["X-Original-Method"].ToString(); // Si lo pasas desde Varnish
    var processedMethod = context.Request.Method;

    string responseContent = $"<h1>HMO Test Endpoint</h1>" +
                             $"<p>Original HTTP Method (as seen by Kestrel): {originalMethod}</p>" +
                             $"<p>Processed HTTP Method (after MethodOverrideMiddleware): {processedMethod}</p>";

    // Puedes añadir lógica aquí para simular PUT/DELETE si lo deseas
    if (processedMethod == "DELETE")
    {
        responseContent += "<p style='color: red;'>!!! SIMULATING RESOURCE DELETION !!!</p>";
    }
    else if (processedMethod == "PUT")
    {
        responseContent += "<p style='color: blue;'>!!! SIMULATING RESOURCE UPDATE !!!</p>";
    }

    await context.Response.WriteAsync(responseContent);
});

app.MapGet("/poisoned-content", async context =>
{
    var hostHeader = context.Request.Headers["Host"].ToString();
    var xForwardedHost = context.Request.Headers["X-Forwarded-Host"].ToString();
    var customHeader = context.Request.Headers["X-Custom-Poison"].ToString(); 
    var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
    var cachedMessageSetting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == "CachedMessage");
    string cachedMessage = cachedMessageSetting?.Value ?? "No message in DB.";

    string responseContent = $"<h1>Cache Poisoning Lab</h1>" +
                             $"<p>Time: {DateTime.UtcNow} UTC</p>" +
                             $"<p>Original Cached Message from DB: {cachedMessage}</p>" +
                             $"<p>Request Host Header: {hostHeader}</p>" +
                             $"<p>X-Forwarded-Host Header: {xForwardedHost}</p>" +
                             $"<p>X-Custom-Poison Header: {customHeader}</p>";
    if (!string.IsNullOrEmpty(customHeader))
    {
        responseContent += $"<p>*** POISONED CONTENT based on X-Custom-Poison: {customHeader} ***</p>";
    }

    context.Response.Headers.Add("Cache-Control", "public, max-age=60"); 
    context.Response.Headers.Add("X-Cache-Poison-Test", "true"); 
    await context.Response.WriteAsync(responseContent);
});


app.Run("http://0.0.0.0:8080");


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AppSetting> Settings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>().ToTable("Settings");
    }
}

public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

