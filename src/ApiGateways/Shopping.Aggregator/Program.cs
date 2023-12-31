using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Shopping.Aggregator.Helper;
using Shopping.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<LoggingDelegatingHandler>();

//Configuring HttpClient for services
builder.Services.AddHttpClient<ICatalogService, CatalogService>(ser =>
    ser.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>()
    .AddPolicyHandler(PolicySetupHelper.GetRetryPolicy())
    .AddPolicyHandler(PolicySetupHelper.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IBasketService, BasketService>(ser =>
    ser.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>()
    //.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(retryCount: 3, _ => TimeSpan.FromSeconds(2)))
    //.AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30)));
    .AddPolicyHandler(PolicySetupHelper.GetRetryPolicy())
    .AddPolicyHandler(PolicySetupHelper.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IOrderService, OrderService>(ser =>
    ser.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]))
    .AddHttpMessageHandler<LoggingDelegatingHandler>()
    .AddPolicyHandler(PolicySetupHelper.GetRetryPolicy())
    .AddPolicyHandler(PolicySetupHelper.GetCircuitBreakerPolicy());

//Adding health checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:CatalogUrl"]}/swagger/index.html"), "Catalog Service", HealthStatus.Degraded)
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:BasketUrl"]}/swagger/index.html"), "Basket Service", HealthStatus.Degraded)
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiSettings:OrderingUrl"]}/swagger/index.html"), "Ordering Service", HealthStatus.Degraded);

var app = builder.Build();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

