using Microsoft.Extensions.DependencyInjection;
using StandConsoleApp.Buisness;
using StandConsoleApp.Buisness.Logger;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TestStandApp.Buisness.Equipment;
using TestStandApp.Connections;
using TestStandApp.ViewModels.Notifications;

namespace TestStandApp
{
    public partial class MainWindow : Window
    {
        private StandViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new StandViewModel();
            DataContext = viewModel;
            //Closed += MainWindow_Closed;
        }



        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //viewModel.ClosePort();

        }

        private void itemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToRightEnd();
        }
    }
}
