using BusinessLogic.Crud;
using BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Data
{
    // Static class to add business logic services to the dependency injection container.
    public static class DbContextRegistration
    {
        // Extension method to register business logic services
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
        {
            // Get the connection string from the ConfigurationService
            string connectionString = ConfigurationService.GetConnectionString();

            // Register AppDbContext with the retrieved connection string
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)
            );

            // Automatic start of migrations
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.Migrate();
                }
            }
            // Register logging services
            services.AddLogging();
            // Register the generic CRUD service for any entity
            services.AddScoped(typeof(ICrud<,>), typeof(Crud<,>));
            // Return the services collection to allow for method chaining
            return services;
        }
    }
}
