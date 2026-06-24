using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SkyFocus.Services;

namespace SkyFocus.DTOs;

public sealed partial class AppRowDto : ObservableObject
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private Bitmap? _icon;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _path = string.Empty;
    [ObservableProperty] private string _processName = string.Empty;
    [ObservableProperty] private string? _iconPath;
    
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private string _noteText = string.Empty;
    
    
    [ObservableProperty] private int _usageTimeSeconds;
}