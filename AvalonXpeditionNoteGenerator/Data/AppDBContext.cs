using System;
using System.Globalization;
using System.IO;
using AvalonXpeditionNoteGenerator.Models;
using Microsoft.Data.Sqlite;
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
        modelBuilder.Entity<MaterialType>(mt =>
        {
            mt.HasKey(x => x.Id);
            mt.HasData(
                MaterialType.Create(1, "Бетон"));
        });
        
        modelBuilder.Entity<ReceiptType>(rt =>
        {
            rt.HasKey(x => x.Id);
            rt.HasData(
                ReceiptType.Create(1, "С 12/15"),
                ReceiptType.Create(2, "С 16/20"),
                ReceiptType.Create(3, "С 20/25"),
                ReceiptType.Create(4, "С 25/30"),
                ReceiptType.Create(5, "С 30/37"),
                ReceiptType.Create(6, "3-ка"),
                ReceiptType.Create(7, "3-ка 6П"),
                ReceiptType.Create(8, "3-ка"),
                ReceiptType.Create(9, "С  8/10 - Зем. Вл."),
                ReceiptType.Create(10, "С 12/15 - Зем. Вл."),
                ReceiptType.Create(11, "С 16/20 - Зем. Вл."),
                ReceiptType.Create(12, "С 20/25 - Зем. Вл."),
                ReceiptType.Create(13, "С 25/30 - Зем. Вл."),
                ReceiptType.Create(14, "С 30/37 - Зем. Вл."),
                ReceiptType.Create(15, "Пясък"),
                ReceiptType.Create(16, "Филц"),
                ReceiptType.Create(17, "Баластра")
            );
        });

        modelBuilder.Entity<DriverType>(dt =>
        {
            dt.HasKey(x => x.Id);
            dt.HasData(
                DriverType.Create(1, "Чавдар Работов")
            );
        });

        modelBuilder.Entity<Truck>(t =>
        {
            t.HasKey(x => x.Id);
            t.HasData(
                Truck.Create(1, "МИКСЕР", "РВ 7980 НА")
            );
        });

        modelBuilder.Entity<GeneratedPdfDocument>(gpd =>
        {
            gpd.HasKey(x => x.Id);
            gpd.HasData(
                GeneratedPdfDocument.Create(App.Settings.InitialNoteNumber, "InitialSpeditionNote"));
        });

        modelBuilder.Entity<ClientType>(cl =>
        {
            cl.HasKey(x => x.Id);
        });
    }
    
    public DbSet<DriverType> Drivers { get; set; }
    public DbSet<MaterialType> Materials { get; set; }
    public DbSet<ReceiptType> Receipts { get; set; }
    public DbSet<Truck> Trucks { get; set; }
    
    public DbSet<ClientType> Clients { get; set; }
    
    public DbSet<GeneratedPdfDocument> PdfDocuments { get; set; }
}