using Serilog;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo 
    {
        Title       = "BugSearch",
        Version     = "v2",
        Description = "BugSearch é um motor de pesquisa de páginas indexadas pelo crawler BugSearch.Crawler.",
        Contact     = new OpenApiContact
        {
            Name  = "BugSearch",
            Email = "andrecastanhal@gmail.com",
            Url   = new Uri("https://github.com/andrrff/BugSearch")
        },
        License = new OpenApiLicense
        {
            Name = "Apache License 2.0",
            Url  = new Uri("https://raw.githubusercontent.com/andrrff/BugSearch/master/LICENSE")
        },
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddCors();

var app = builder.Build();

int workerThreads, completionPortThreads;
ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
ThreadPool.SetMinThreads(workerThreads, completionPortThreads);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .WriteTo.Console()
    .CreateLogger();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
