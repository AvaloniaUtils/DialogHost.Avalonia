using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DialogHostAvalonia;
using DialogHostDemo.Models;
using static DialogHostAvalonia.DialogHost;

namespace DialogHostDemo.Views {
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

        private async void OpenDialogWithView(object? sender, RoutedEventArgs e) {
            await DialogHost.Show(Resources["Sample3View"]!, "MainDialogHost");
        }
        
        private async void OpenDialogWithModel(object? sender, RoutedEventArgs e) {
            await DialogHost.Show(new Sample2Model(new Random().Next(0, 100)), "MainDialogHost");
        }

        private async void OpenNoAnimationDialog(object? sender, RoutedEventArgs e) {
            await DialogHost.Show(Resources["Sample2View"]!, "NoAnimationDialogHost");
        }
    }
}