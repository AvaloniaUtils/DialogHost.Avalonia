using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;
using DialogHostDemo.Models;

namespace DialogHostDemo.Views;

public partial class MainWindow : Window {
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

    private async void OpenBlurDialog(object? sender, RoutedEventArgs e) {
        await DialogHost.Show(Resources["Sample2View"]!, "BlurDialogHost");
    }
}