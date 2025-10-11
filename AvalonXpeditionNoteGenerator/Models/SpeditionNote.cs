using System;
using AvalonXpeditionNoteGenerator.Config;

namespace AvalonXpeditionNoteGenerator.Models;

public class SpeditionNote
{
    public int NoteNumber { get; set; }
    
    public string Client { get; set; }
    public decimal Quantity { get; set; }
    
    public bool AddedWater { get; set; }
    
    public ReceiptType SelectedReceipt { get; set; }

    public MaterialType SelectedMaterial   { get; set; }    
    public Truck SelectedTruck { get; set; } 
    

    public DriverType SelectedDriver { get; set; } 
    
    public string PdfDocumentPath { get; private set; }

    private SpeditionNote(int number, decimal quantity, bool water, ReceiptType receipt, MaterialType material, Truck truck, DriverType driver)
    {
        NoteNumber = number;
        Quantity = quantity;
        AddedWater = water;
        Client = string.Empty;
        SelectedReceipt = receipt;
        SelectedMaterial = material;
        SelectedTruck = truck;
        SelectedDriver = driver;
        PdfDocumentPath = String.Empty;
    }
    public static SpeditionNote Create(int number, decimal quantity, bool water, ReceiptType receipt, MaterialType material, Truck truck, DriverType driver) 
        => new (number, quantity, water, receipt, material, truck, driver);
    
    public void SetDocumentPath() => PdfDocumentPath = AppConfig.PdfPath($"Note_{NoteNumber.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    public string NoteCaption => $"Eкспедиционна бележка";
    
    public string NoteDate =>  $"  / {DateTime.Now:g}";
    
};