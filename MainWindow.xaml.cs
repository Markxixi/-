using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartNotes.Models;
using SmartNotes.ViewModels;

namespace SmartNotes
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        private void NoteCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Note note)
            {
                _viewModel.SelectedNote = note;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ApiKey = ApiKeyBox.Password;
            _viewModel.SaveApiSettingsCommand.Execute(null);
        }
    }
}
