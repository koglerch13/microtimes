using System;
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

    public event EventHandler? UserInput; 

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

    private void DatePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        InnerDatePicker.IsDropDownOpen = !InnerDatePicker.IsDropDownOpen;
    }
    
    private void TextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        UserInput?.Invoke(this, EventArgs.Empty);
    }
}