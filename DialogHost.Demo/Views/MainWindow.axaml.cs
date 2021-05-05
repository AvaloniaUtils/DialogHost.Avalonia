using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace DialogHost.Demo.Views {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            #if DEBUG
            this.AttachDevTools();
            #endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private async void MainDialogHost_OnDialogOpened(object sender, DialogOpenedEventArgs eventargs) {
            // if (sender is Avalonia.DialogHost host) {
            //     await Task.Delay(1000);
            //     host.IsOpen = false;
            // }
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e) {
            DialogHost.GetDialogSession("DialogHost.Identifier here")?.Close(false);
            DialogHost.Show(new Rectangle() {Height = 10, Width = 20, Fill = Brushes.Aqua}, "123",
                (object o, DialogOpenedEventArgs args) => { Debug.WriteLine("123"); }, (o, args) => Debug.WriteLine("321"));
        }
    }
}