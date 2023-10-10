using Microsoft.Extensions.DependencyInjection;
using TestStandApp.Buisness.Logger;
using TestStandApp.Buisness;
using System.Windows;
using TestStandApp.Buisness.Equipment;
using TestStandApp.Connections;
using TestStandApp.ViewModels.Notifications;
using System.ComponentModel;

namespace TestStandApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceCollection Services = new ServiceCollection();
            Services = ConfigureServices(Services);

            var serviceProvider = Services.BuildServiceProvider();

            var window = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<StandViewModel>()
            };

            window.Show();
        }

        public ServiceCollection ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<Scenario>();
            services.AddSingleton<SerialPortConnection>();
            services.AddSingleton<LanConnection>();
            services.AddSingleton<Belt>();
            services.AddSingleton<Shutter>();
            services.AddSingleton<Generator>();
            services.AddSingleton<Button>();
            services.AddSingleton<Detector>();
            services.AddSingleton<StandViewModel>();
            return services;
        }
    }
}
