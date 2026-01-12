using System.ComponentModel.DataAnnotations.Schema;

namespace AvalonXpeditionNoteGenerator.Models;

public class DriverType 
{
    // By convention, a property named Id or <type name>Id will be configured as the primary key of an entity.
    public int Id { get; init; }
    
    [Column(TypeName = "nvarchar(200)")]
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