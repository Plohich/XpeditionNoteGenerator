using System.ComponentModel.DataAnnotations.Schema;

namespace AvalonXpeditionNoteGenerator.Models;

public class ClientType
{
    // By convention, a property named Id or <type name>Id will be configured as the primary key of an entity.
    public int Id { get; init; }
    
    [Column(TypeName = "nvarchar(250)")]
    public string ClientName { get; set; } = string.Empty;
    public static ClientType Create(int id, string name) => new()
    {
        Id = id,
        ClientName = name   
    };
    
    public static ClientType CreateWithName(string name) => new()
    {
        ClientName = name   
    };
}