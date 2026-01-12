using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvalonXpeditionNoteGenerator.Models;

public class GeneratedPdfDocument 
{
    public int Id { get; init; }
    
    [Column(TypeName = "nvarchar(300)")]
    public required string PdfPath { get; init; }

    public static GeneratedPdfDocument Create(int id, string pdfPath) => new()
    {
        Id = id,
        PdfPath = pdfPath,
    };
}