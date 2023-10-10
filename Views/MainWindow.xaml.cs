using System.Windows;

namespace TestStandApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void itemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToRightEnd();
        }
        
        private void itemsControlForScenario_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewerForScenario.ScrollToRightEnd();
        }
    }
}
