using FaturamentoService.Data;
using FaturamentoService.Repositories;
using FaturamentoService.Repositories.Interfaces;
using FaturamentoService.Services;
using FaturamentoService.Services.Interfaces;
using FaturamentoService.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<EmitirNotaFiscalRequestValidator>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<FaturamentoDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (result, delay, attempt, _) =>
                Log.Warning("Tentativa {Attempt}/3 de conectar ao EstoqueService. Aguardando {Delay}s... ({Reason})",
                    attempt, delay.TotalSeconds, result.Exception?.Message ?? result.Result?.StatusCode.ToString()));

    var circuitBreakerPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (_, duration) =>
                Log.Warning("EstoqueService indisponível. Circuit aberto por {Duration}s.", duration.TotalSeconds),
            onReset: () =>
                Log.Information("EstoqueService disponível novamente. Circuit fechado."),
            onHalfOpen: () =>
                Log.Information("Testando disponibilidade do EstoqueService..."));

    builder.Services.AddScoped<INotaFiscalRepository, NotaFiscalRepository>();
    builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

    builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["EstoqueService:BaseUrl"] ?? "http://localhost:5189");
    })
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200"
            )
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type")
    );
});

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FaturamentoDbContext>();
        db.Database.EnsureCreated();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
