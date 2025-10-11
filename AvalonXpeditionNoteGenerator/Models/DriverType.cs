namespace AvalonXpeditionNoteGenerator.Models;

public class DriverType 
{
    public int Id { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public static DriverType Create(int id, string name) => new()
    {
        Id = id,
        DriverName = name   
    };
    
    public static DriverType CreateWithName(string name) => new()
    {
        DriverName = name   
    };
}