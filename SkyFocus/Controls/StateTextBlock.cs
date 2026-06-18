using System;
using Avalonia;
using Avalonia.Controls;

namespace SkyFocus.Controls;

public class StateTextBlock : TextBlock
{
    static StateTextBlock()
    {
        IsCheckedProperty.Changed.AddClassHandler<StateTextBlock>((x, e) => x.UpdatePseudoClasses());
    }

    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<StateTextBlock, bool>(nameof(IsChecked), false);

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly StyledProperty<string> ActiveTextProperty =
        AvaloniaProperty.Register<StateTextBlock, string>(nameof(ActiveText), "● Запущено");

    public string ActiveText
    {
        get => GetValue(ActiveTextProperty);
        set => SetValue(ActiveTextProperty, value);
    }

    public static readonly StyledProperty<string> InactiveTextProperty =
        AvaloniaProperty.Register<StateTextBlock, string>(nameof(InactiveText), "○ Не запущено");

    public string InactiveText
    {
        get => GetValue(InactiveTextProperty);
        set => SetValue(InactiveTextProperty, value);
    }

    public StateTextBlock()
    {
        // Устанавливаем начальный текст
        UpdateText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
            
        if (change.Property == IsCheckedProperty || 
            change.Property == ActiveTextProperty || 
            change.Property == InactiveTextProperty)
        {
            UpdateText();
        }
    }

    private void UpdateText()
    {
        Text = IsChecked ? ActiveText : InactiveText;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":checked", IsChecked);
    }
}