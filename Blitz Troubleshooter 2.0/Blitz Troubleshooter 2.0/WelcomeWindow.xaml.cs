using System.Windows;

namespace Blitz_Troubleshooter_2._0
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Window1.Title = "Version 0.5";
        }
       

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            var troubleshootingpage = new TroubleshootPage(e.Source.GetHashCode());
            troubleshootingpage.ShowDialog();
        }
    }

}
