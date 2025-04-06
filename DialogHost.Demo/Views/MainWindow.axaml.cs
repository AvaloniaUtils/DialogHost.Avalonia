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
    
    private async void OpenNoAnimationDialog(object? sender, RoutedEventArgs e) {
        await DialogHost.Show(Resources["Sample2View"]!, "NoAnimationDialogHost");
    }

    private async void OpenDialogWithView(object? sender, RoutedEventArgs e) {
        await DialogHost.Show(Resources["Sample3View"]!, "MainDialogHost");
    }

    private void CloseDialogWithModel(object? sender, RoutedEventArgs e) {
        DialogHost.Close("NoAnimationDialogHost");
    }

    private void OffDialogWithModel(object? sender, RoutedEventArgs e) {
        NoAnimationDialogHost.IsOpen = false;
    }

    private void OnDialogWithModel(object? sender, RoutedEventArgs e) {
        NoAnimationDialogHost.IsOpen = true;
    }

    private async void OpenDialogWithModel(object? sender, RoutedEventArgs e) {
        await DialogHost.Show(new Sample3Model(new Random().Next(0, 100)), "NoAnimationDialogHost");
    }

    private async void OpenBlurDialog(object? sender, RoutedEventArgs e) {
        await DialogHost.Show(Resources["Sample4View"]!, "BlurDialogHost");
    }
}