namespace AvalonXpeditionNoteGenerator.Models;

public class MaterialType
{
    public int Id { get; set; }
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