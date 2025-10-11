using System.Data.Common;

namespace AvalonXpeditionNoteGenerator.Models;

public class ReceiptType 
{
    public int Id
    {
        get; set;
    }

    public string Description
    {
        get; set;
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