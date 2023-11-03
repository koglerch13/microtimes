using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MicroTimes;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TimeInput_OnFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Dispatcher.UIThread.Post(() =>
            {
                textBox.SelectAll();
            });
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        InnerDatePicker.IsDropDownOpen = !InnerDatePicker.IsDropDownOpen;
    }
}