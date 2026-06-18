namespace SkyFocus.Data.Entities;

public class AppEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    
    public bool IsFavorite { get; set; }
}