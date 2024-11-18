using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Data
{
    public static class ConfigurationService
    {
        // Static variable to store the loaded configuration once to avoid reloading multiple times
        private static IConfiguration? _configuration;

        // Method to get the configuration object
        public static IConfiguration GetConfiguration()
        {
            // If configuration is not already loaded, load it from appsettings.json
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())  // Set the base path for the configuration file
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Load the appsettings.json file
                    .Build();
            }
            return _configuration;
        }

        // Method to get the connection string from the configuration
        public static string GetConnectionString()
        {
            // Retrieve the configuration and get the connection string named "DefaultConnection"
            var configuration = GetConfiguration();
            return configuration.GetConnectionString("DefaultConnection") ??
                   throw new InvalidOperationException("DefaultConnection string not found in appsettings.json");
        }
    }
}
