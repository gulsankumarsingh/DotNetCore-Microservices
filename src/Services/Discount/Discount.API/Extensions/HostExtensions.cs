﻿using Microsoft.Extensions.Configuration;
using Npgsql;
using Polly;
using Serilog;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host)
        {

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrating postresql database");

                    //Handling retry using Polly
                    var retry = Policy.Handle<NpgsqlException>()
                        .WaitAndRetry(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, retryCount, context) =>
                        {
                            Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                        });

                    retry.Execute(() => ExecuteMigrations(configuration));

                    logger.LogInformation("Migrated postresql database");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occured while migrating the postgresql database");
                }
            }
            return host;
        }

        private static void ExecuteMigrations(IConfiguration configuration)
        {
            NpgsqlConnection connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand
            {
                Connection = connection
            };
            command.CommandText = "DROP TABLE IF EXISTS Coupon";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE Coupon (Id SERIAL PRIMARY KEY,
                                                                    ProductName VARCHAR(24) NOT NULL,
                                                                    Description TEXT,
                                                                    Amount INT)";
            command.ExecuteNonQuery();

            command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) Values ('IPhone X', 'IPhone Discount', 150);";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) Values ('Samsung 10', 'Samsung Discount', 100);";
            command.ExecuteNonQuery();
        }
    }
}
