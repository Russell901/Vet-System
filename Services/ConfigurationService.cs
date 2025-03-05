using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Vet_System.Services
{
    public static class ConfigurationService
    {
        private static IConfiguration _configuration;

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    try
                    {
                        var appLocation = AppContext.BaseDirectory;
                        var builder = new ConfigurationBuilder()
                            .SetBasePath(appLocation)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                        _configuration = builder.Build();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Configuration error: {ex.Message}");
                        // Provide default configuration if file is not found
                        _configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>
                            {
                                {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=vet_system;Uid=root;Pwd=;"},
                            })
                            .Build();
                    }
                }
                return _configuration;
            }
        }

        public static string ConnectionString
        {
            get
            {
                try
                {
                    return Configuration.GetConnectionString("DefaultConnection")
                        ?? "Server=localhost;Database=vet_system;Uid=root;Pwd=;";
                }
                catch
                {
                    return "Server=localhost;Database=vet_system;Uid=root;Pwd=;";
                }
            }
        }
    }
}
