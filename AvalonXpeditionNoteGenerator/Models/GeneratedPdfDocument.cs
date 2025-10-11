using System;

namespace AvalonXpeditionNoteGenerator.Models;

public class GeneratedPdfDocument 
{
    public int Id { get; set; }
    public required string PdfPath { get; set; }

    public static GeneratedPdfDocument Create(int id, string pdfPath) => new()
    {
        Id = id,
        PdfPath = pdfPath,
    };
}