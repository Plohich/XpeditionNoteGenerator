using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace AvalonXpeditionNoteGenerator.Models;

public class ReceiptType 
{
    // By convention, a property named Id or <type name>Id will be configured as the primary key of an entity.
    public int Id
    {
        get; init;
    }
    
    [Column(TypeName = "nvarchar(200)")]
    public string Description
    {
        get; init;
    } = string.Empty;
    public static ReceiptType Create(int id, string description) => new ()
    {
        Id = id,
        Description = description
    };
    public static ReceiptType CreateWithDescription(string description) => new ()
    {
        Description = description
    };
    
    public string FullDescription => $"{Id} - {Description}";
};