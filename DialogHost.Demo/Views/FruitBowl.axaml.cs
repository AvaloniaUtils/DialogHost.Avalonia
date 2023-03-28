﻿using System.Diagnostics;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DialogHostAvalonia;
using DialogHostAvalonia.DialogEvents;

namespace DialogHostDemo.Views {
    public partial class FruitBowl : UserControl {
        public AvaloniaList<string> ListBoxSource = new AvaloniaList<string>() {
            "Apple",
            "Banana",
            "Peer"
        };
        public FruitBowl() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
            ListBox fruitListBox = this.FindControl<ListBox>("FruitListBox");
            fruitListBox.Items = ListBoxSource;
        }

        private void FruitBowl_OnDialogClosing(object? sender, DialogClosingEventArgs e) {
            Debug.WriteLine($"SAMPLE 1: Closing dialog with parameter: {e.Parameter ?? string.Empty}");

            //you can cancel the dialog close:
            //eventArgs.Cancel();

            if (!string.IsNullOrWhiteSpace(e.Parameter?.ToString()))
                ListBoxSource.Add(e.Parameter.ToString()!.Trim());
        }
    }
}