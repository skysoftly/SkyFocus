using SkyFocus.DTOs;

namespace SkyFocus.Models;

public class TopApp
{
    public AppRowDto App { get; set; }
    public int TotalSeconds { get; set; }
    public int Rank { get; set; }
}