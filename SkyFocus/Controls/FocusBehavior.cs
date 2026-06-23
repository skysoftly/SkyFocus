using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace SkyFocus.Controls;

public static class FocusBehavior
{
    public static readonly AttachedProperty<bool> IsFocusedProperty =
        AvaloniaProperty.RegisterAttached<object, Control, bool>("IsFocused", 
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static void SetIsFocused(Control element, bool value) => element.SetValue(IsFocusedProperty, value);
    public static bool GetIsFocused(Control element) => element.GetValue(IsFocusedProperty);

    static FocusBehavior()
    {
        IsFocusedProperty.Changed.AddClassHandler<Control>((control, e) =>
        {
            // ✅ ТОЛЬКО ДЛЯ TEXTBOX
            if (control is not TextBox) return;
            
            bool isFocused = (bool)e.NewValue!;
            if (isFocused && control.IsVisible)
            {
                Dispatcher.UIThread.Post(() => control.Focus(), DispatcherPriority.Loaded);
            }
        });

        InputElement.GotFocusEvent.AddClassHandler<Control>((control, e) =>
        {
            // ✅ ТОЛЬКО ДЛЯ TEXTBOX
            if (control is TextBox)
            {
                SetIsFocused(control, true);
            }
        });

        InputElement.LostFocusEvent.AddClassHandler<Control>((control, e) =>
        {
            // ✅ ТОЛЬКО ДЛЯ TEXTBOX
            if (control is TextBox)
            {
                SetIsFocused(control, false);
            }
        });
    }
}