using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace BusinessLogic.Data
{
    // Factory class used for creating an instance of AppDbContext at design time,useful for migrations.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        // CreateDbContext to create the AppDbContext instance.
        public AppDbContext CreateDbContext(string[] args)
        {
            // Retrieve the connection string using the ConfigurationService
            var connectionString = ConfigurationService.GetConnectionString();

            // Configure the DbContext with the retrieved connection string
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            // Return a new instance of AppDbContext with the configured options
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
