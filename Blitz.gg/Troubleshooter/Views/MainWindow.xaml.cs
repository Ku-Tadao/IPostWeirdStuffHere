using BlitzTroubleshooter.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;

namespace BlitzTroubleshooter
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;


        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        // New event handlers
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.LoadAsync();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            Application.Current.Shutdown();
        }
    }
}