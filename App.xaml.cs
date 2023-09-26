using Microsoft.Extensions.DependencyInjection;
using StandConsoleApp.Buisness.Logger;
using StandConsoleApp.Buisness;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TestStandApp.Buisness.Equipment;
using TestStandApp.Connections;
using TestStandApp.ViewModels.Notifications;

namespace TestStandApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public ServiceCollection Services;

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    CreateInstances();

        //    var mainWindow = new MainWindow();
        //    var serviceProvider = Services.BuildServiceProvider();

        //    mainWindow.DataContext = serviceProvider.GetRequiredService<StandViewModel>();
        //    mainWindow.Show();
        //}

        //private void CreateInstances()
        //{
        //    Services = new ServiceCollection();
        //    Services = ConfigureServices(Services);
        //}

        //public ServiceCollection ConfigureServices(ServiceCollection services)
        //{
        //    services.AddSingleton<ILogger, ConsoleLogger>();
        //    services.AddSingleton<Scenario>();
        //    services.AddSingleton<SerialPortConnection>();
        //    services.AddSingleton<LanConnection>();
        //    //services.AddSingleton<Belt>();
        //    //services.AddSingleton<Shutter>();
        //    //services.AddSingleton<Generator>();
        //    services.AddSingleton<Button>();
        //    services.AddSingleton<Detector>();
        //    services.AddSingleton<StandViewModel>();
        //    return services;
        //}
    }
}
