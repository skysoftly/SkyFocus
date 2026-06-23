using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SkyFocus.ViewModels;

namespace SkyFocus.Views;

public partial class WindowBarView : UserControl
{
    public WindowBarView()
    {
        InitializeComponent();
    }
    
    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        App.MainWindow?.BeginMoveDrag(e);
    }
}