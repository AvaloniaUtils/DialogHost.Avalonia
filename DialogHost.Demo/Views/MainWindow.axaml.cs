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

        private void OpenDialogWithView(object? sender, RoutedEventArgs e) {
            DialogHost.Show(Resources["Sample3View"]!, "MainDialogHost");
        }
        
        private void OpenDialogWithModel(object? sender, RoutedEventArgs e) {
            DialogHost.Show(new Sample2Model(new Random().Next(0, 100)), "MainDialogHost");
        }

        private void OpenNoAnimationDialog(object? sender, RoutedEventArgs e) {
            DialogHost.Show(Resources["Sample2View"]!, "NoAnimationDialogHost");
        }
    }
}