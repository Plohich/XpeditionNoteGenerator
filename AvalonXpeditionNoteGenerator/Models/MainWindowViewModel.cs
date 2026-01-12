using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvalonXpeditionNoteGenerator.Config;
using AvalonXpeditionNoteGenerator.Data;
using AvalonXpeditionNoteGenerator.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.EntityFrameworkCore;

namespace AvalonXpeditionNoteGenerator.Models;

public partial class MainWindowViewModel : ObservableObject
{
    // Using ObservableCollection is better for granular updates, when MainViewModel could have too big sub-collections(could cause UI issues)
    // But for now will stick to complete update of ViewModel, once some entity has been added just reload whole model
    private ObservableCollection<ReceiptType> _receiptTypes = new();
    public ObservableCollection<ReceiptType> Receipts 
    {
        get;
        private set => SetProperty(ref field, value);
    }
    
    public List<Truck> Trucks { get; init; } 

    public List<DriverType> Drivers { get; init; } 

    public List<MaterialType> Materials { get; init; } 
    
    public SpeditionNote PrintableExpeditionNote { get; }
    
    public NewEntity EntityToAdd { get; }
    
    private Report SpeditionNoteReport { get;  } = new ();

    [ObservableProperty]
    private bool isLoading;

    private  MainWindowViewModel(List<ReceiptType> receipts, List<Truck> trucks, List<DriverType> drivers, List<MaterialType> materials, int initialNoteNumber)
    {
        Receipts = new (receipts);
        Trucks = trucks;
        Drivers = drivers;
        Materials = materials;
        PrintableExpeditionNote = SpeditionNote.Create(initialNoteNumber, 1M, false, receipts[0], materials[0], trucks[0], drivers[0]);
        EntityToAdd = NewEntity.Create();
        
        SpeditionNoteReport.Load(AppConfig.ReportTemplatePath);
    }
    
    public static  async Task<MainWindowViewModel> LoadData()
    {
            
        await using var db = new AppDbContext(AppConfig.DatabasePath);
        var receipts = await db.Receipts.ToListAsync();
        var trucks = await db.Trucks.ToListAsync();
        var drivers = await db.Drivers.ToListAsync();
        var materials = await db.Materials.ToListAsync();
        int maxGeneratedDocId = await db.PdfDocuments.AnyAsync() ? db.PdfDocuments.Max(d => d.Id) : App.Settings.InitialNoteNumber;
            
        MainWindowViewModel vm = new MainWindowViewModel( receipts, trucks, drivers, materials, maxGeneratedDocId);
            
        return vm;
            
    }

    public async Task<bool> RefreshReceiptDataSource()
    {
        bool result;
        try
        {
            await using var db = new AppDbContext(AppConfig.DatabasePath);
            var receipts = await db.Receipts.ToListAsync();
            Receipts = new (receipts);
            PrintableExpeditionNote.SelectedReceipt = Receipts[^1];
            result = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return result;
    }
    
    public static async Task<IEnumerable<string>> GetClientsDataSource(string term, CancellationToken cncToken)
    {
        try
        {
            await using var db = new AppDbContext(AppConfig.DatabasePath);
            // var temp = await db.Clients
            //     .Where(c => EF.Functions.Like(c.ClientName, $"%{term}%"))
            //     .Select(c => c.ClientName)
            //     .Take(10)
            //     .ToListAsync(cncToken);
            // var temp = await db.Clients
            //     .AsNoTracking()
            //     .Where(p => EF.Functions.Collate(p.ClientName, "NOCASE") // Use the custom name you registered
            //         .Contains(term))
            //     .Select(p => p.ClientName)
            //     .OrderBy(p => p)
            //     .Take(10)
            //     .ToListAsync(cncToken);

            var temp = await db.Clients
                .AsNoTracking()
                .Select(c => c.ClientName)
                .ToListAsync(cncToken);
            return temp.Where(client => client.Contains(term, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(client => client)
                .Take(15);
        }
        catch (OperationCanceledException)
        {
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while getting clients data source.");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> PrepareReport()
    {
         IsLoading = true;
                    

                        SpeditionNoteReport.SetParameterValue("NoteNumberParam", PrintableExpeditionNote.NoteNumber.ToString());
                        SpeditionNoteReport.SetParameterValue("ReceiptNumParam", PrintableExpeditionNote.SelectedReceipt.Id.ToString());
                        SpeditionNoteReport.SetParameterValue("ReceiptCodeParam", PrintableExpeditionNote.SelectedReceipt.Description.ToUpperInvariant());
                        SpeditionNoteReport.SetParameterValue("ProductParam",
                            PrintableExpeditionNote.SelectedMaterial.MaterialDescription);
                        SpeditionNoteReport.SetParameterValue("NoteDateParam", DateTime.Now);
                        SpeditionNoteReport.SetParameterValue("QtyParam", PrintableExpeditionNote.Quantity);
                        SpeditionNoteReport.SetParameterValue("ClientParam", PrintableExpeditionNote.Client.ToUpperInvariant());
                        SpeditionNoteReport.SetParameterValue("VehicleNoParam", PrintableExpeditionNote.SelectedTruck.TruckDescription);
                        SpeditionNoteReport.SetParameterValue("DriverNameParam", PrintableExpeditionNote.SelectedDriver.DriverName.ToUpperInvariant());
                        SpeditionNoteReport.SetParameterValue("AddedWaterParam", PrintableExpeditionNote.AddedWater);

        
                       var prepared =  await Task.Run(() =>
                        {
                            bool result =  SpeditionNoteReport.Prepare();
                           
                            //using var stream = new MemoryStream();
                            // speditionNoteReport.Export(pdfExport, stream);
                            // await Printer.ShowPrintDialogAsync(stream.ToArray());
                            // await IronPrint.Printer.PrintAsync(stream.ToArray());

                            if (result)
                            {
                                PrintableExpeditionNote.SetDocumentPath();
                                SpeditionNoteReport.Export(new PDFSimpleExport(), PrintableExpeditionNote.PdfDocumentPath);
                            }

                            return result;
                        });  
        IsLoading = false;
        return prepared;
    }

    public async Task<bool> SaveEntity(ModelTypeEnum modelType)
    {
        bool result = false;
        try
        {
            await using var db = new AppDbContext(AppConfig.DatabasePath);
            object entry = modelType switch
            {
                ModelTypeEnum.SpeditionNote =>  db.PdfDocuments.Add(new GeneratedPdfDocument()
                {
                    PdfPath = PrintableExpeditionNote.PdfDocumentPath,
                }),
                
                ModelTypeEnum.Receipt =>  db.Receipts.Add(ReceiptType.CreateWithDescription(EntityToAdd.ReceiptCode)),
                ModelTypeEnum.Client => db.Clients.Add(ClientType.CreateWithName(EntityToAdd.ClientName)),
                ModelTypeEnum.Driver => db.Drivers.Add(DriverType.CreateWithName((EntityToAdd.DriverName))),
                ModelTypeEnum.Material => db.Materials.Add(MaterialType.CreateWithDesc(EntityToAdd.MaterialCode)),
                ModelTypeEnum.Truck => db.Trucks.Add(Truck.CreateWithTypeAndReg(EntityToAdd.TruckClass, EntityToAdd.TruckRegNumber)),
                _ => throw new InvalidCastException("Unknown model type")
            };
        
            await db.SaveChangesAsync();
            result = true;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return result;
    }
    
    
}