using System.ComponentModel.DataAnnotations.Schema;

namespace AvalonXpeditionNoteGenerator.Models;

public class MaterialType
{
    // By convention, a property named Id or <type name>Id will be configured as the primary key of an entity.
    public int Id { get; init; }
    
    [Column(TypeName = "nvarchar(200)")]
    public string MaterialDescription { get; set; } = string.Empty;

    public static MaterialType Create(int id, string materialDescription) => new()
    {
        Id = id,
        MaterialDescription = materialDescription
    };
    public static MaterialType CreateWithDesc(string materialDescription) => new()
    {
        MaterialDescription = materialDescription
    };
};