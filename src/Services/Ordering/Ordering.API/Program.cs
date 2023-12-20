using Common.Logging;
using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.EventBusConsumer;
using Ordering.API.Extensions;
using Ordering.Application.StartupExtensions;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.StatupExtensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

//MassTransit-RabbitMq Configuration
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<BasketCheckoutConsumer>();

    config.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration.GetValue<string>("EventBusSettings:HostAddress"));
        configurator.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>       //Subscribing to queue
        {
            c.ConfigureConsumer<BasketCheckoutConsumer>(context);
        });
    });
});

//builder.Services.Configure<MassTransitHostOptions>(options =>
//{
//    options.WaitUntilStarted = true;                //Application won't start until rabbitmq is connected
//    options.StartTimeout = TimeSpan.FromSeconds(30);
//    options.StopTimeout = TimeSpan.FromMinutes(1);
//});

//General Configuration
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<BasketCheckoutConsumer>();

var app = builder.Build();
app.UseSerilogRequestLogging();
app.MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetService<ILogger<OrderContextSeed>>();
    OrderContextSeed.SeedAsync(context, logger).Wait();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
