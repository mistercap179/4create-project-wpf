using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Data;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using BusinessLogic;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using WpfClient.ViewModels;
using WpfClient.Views;
using log4net;
using log4net.Config;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // This property holds the service provider for dependency injection
        public static IServiceProvider ServiceProvider { get; private set; }
        
        // Static logger field
        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        // This method is called when the application starts
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Log4Net from the configuration file
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            // Create a new service collection (DI container)
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            serviceCollection.AddBusinessLogic();  

            // Build the ServiceProvider from the service collection
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Request the EmployeeView from DI container
            var employeeView = ServiceProvider.GetRequiredService<EmployeeView>();

            // Request the EmployeeViewModel from DI container
            var employeeViewModel = ServiceProvider.GetRequiredService<EmployeeViewModel>();

            // Set the DataContext of the EmployeeView to the EmployeeViewModel
            employeeView.DataContext = employeeViewModel;

            // Show the EmployeeView window
            employeeView.Show();            
            
            // Log an informational message at the start
            log.Info("Application Starting");
        }

        // Method to configure and register services in the DI container
        private void ConfigureServices(IServiceCollection services)
        {
            // Register AutoMapper for object mapping
            IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
            services.AddSingleton(mapper);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Register EmployeeView and EmployeeViewModel with their respective lifetimes
            services.AddSingleton<EmployeeView>(); 
            services.AddScoped<EmployeeViewModel>(); // EmployeeViewModel is Scoped since it depends on services
        }
    }
}
