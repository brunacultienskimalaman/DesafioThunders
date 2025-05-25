using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Thunders.TechTest.ApiService;
using Thunders.TechTest.ApiService.Data;
using Thunders.TechTest.ApiService.Services;
using Thunders.TechTest.OutOfBox.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Fixando portas da api
builder.WebHost.UseUrls("https://localhost:7000", "http://localhost:7001");

var features = Features.BindFromConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.AddSqlServerDbContext<AppDbContext>("ThundersTechTestDb", configureDbContextOptions: options =>
{
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Sistema de Relatórios de Pedágio",
        Version = "v1",
        Description = "API para processamento de dados de utilizações de pedágio e geração de relatórios"
    });
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


builder.Services.AddScoped<IUtilizacaoService, UtilizacaoService>();
builder.Services.AddScoped<IPracaService, PracaService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            }));
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddSource("PedagioSystem");
    });

var app = builder.Build();

//Precisei adicionar isso aqui para forcar as migrações, não estavam funcionando 
//chamando via console externo
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pedágio API v1");
        c.RoutePrefix = "";
    });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    await next();
    stopwatch.Stop();

    if (stopwatch.ElapsedMilliseconds > 5000)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Request lenta: {Method} {Path} - {ElapsedMs}ms",
            context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
    }
});

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();



