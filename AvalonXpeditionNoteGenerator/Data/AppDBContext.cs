using System;
using System.IO;
using AvalonXpeditionNoteGenerator.Models;
using Microsoft.EntityFrameworkCore;

namespace AvalonXpeditionNoteGenerator.Data;

public class AppDbContext : DbContext
{
    private string DbPath { get; }

    public AppDbContext(string dbPath)
    {
        DbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
            
        // Seed initial data
        modelBuilder.Entity<MaterialType>().HasData(
            MaterialType.Create(1, "Бетон")
        );
            
        modelBuilder.Entity<ReceiptType>().HasData(
            ReceiptType.Create(1, "С 20/25 S3 Ш1")
        );

        modelBuilder.Entity<DriverType>().HasData(
            DriverType.Create(1, "Чавдар Работов")
            );
        modelBuilder.Entity<Truck>().HasData(
            Truck.Create(1, "МИКСЕР", "РВ 7980 НА")
        );
        modelBuilder.Entity<GeneratedPdfDocument>().HasData(
            GeneratedPdfDocument.Create(App.Settings.InitialNoteNumber, "InitialSpeditionNote"));
    }
    
    public DbSet<DriverType> Drivers { get; set; }
    public DbSet<MaterialType> Materials { get; set; }
    public DbSet<ReceiptType> Receipts { get; set; }
    public DbSet<Truck> Trucks { get; set; }
    
    public DbSet<GeneratedPdfDocument> PdfDocuments { get; set; }
}