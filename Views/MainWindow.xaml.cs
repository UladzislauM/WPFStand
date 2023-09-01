using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TestStandApp.ViewModels.Notifications;

namespace TestStandApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StandViewModel viewModel = new StandViewModel();
            DataContext = viewModel;
           
        }
    }
}
