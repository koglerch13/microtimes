using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
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
}