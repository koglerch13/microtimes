using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

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

    private void ActiveEntryDescription_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("focusLost");
    }

    private void ActiveEntryDescription_OnKeyUp(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.Escape:
                FocusManager?.ClearFocus();
                break;
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (!_focusNewTimeEntry)
            return;
        
        var descriptionTextBox = control.FindDescendantOfType<TextBox>();
        if (descriptionTextBox == null)
            return;

        descriptionTextBox.Focus();
        control.BringIntoView();
    }

    private bool _focusNewTimeEntry;
    public void FocusNewTimeEntry()
    {
        _focusNewTimeEntry = true;
        Dispatcher.UIThread.Post(() => _focusNewTimeEntry = false);
    }
}