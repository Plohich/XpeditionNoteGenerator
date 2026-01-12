using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvalonXpeditionNoteGenerator.Config;
using AvalonXpeditionNoteGenerator.Controls;
using AvalonXpeditionNoteGenerator.Data;
using AvalonXpeditionNoteGenerator.Models;
using AvalonXpeditionNoteGenerator.Enums;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static AvalonXpeditionNoteGenerator.Constants.Constants;

namespace AvalonXpeditionNoteGenerator;

public partial class MainWindow : Window
{
    private CancellationTokenSource? currentCts;

    public MainWindow()
    {
        InitializeComponent();

        // Configure Serilog once at application startup
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "logs/app-.txt",
                rollingInterval: RollingInterval.Day, // Creates new file each day
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        Loaded += async (sender, e) => { DataContext = await LoadData(); };

        ClientNameAutoComplete.AsyncPopulator += async (term, token) =>
        {
            currentCts?.CancelAsync();
            currentCts?.Dispose();
            currentCts = CancellationTokenSource.CreateLinkedTokenSource(token);

            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Enumerable.Empty<object>();
            }

            // Call  will be cancelled if user types again
            var result = await MainWindowViewModel.GetClientsDataSource(term, currentCts.Token);

            // AutoCompleteBox expects IEnumerable<object>
            return result;
        };

        Closing += OnWindowClosing;
    }

    private async Task<MainWindowViewModel> LoadData() => await MainWindowViewModel.LoadData(Log.ForContext<MainWindowViewModel>());

    private async Task<bool> InsertNewNoteNumber()
    {
        if (DataContext is MainWindowViewModel
            {
                PrintableExpeditionNote:
                {
                    PdfDocumentPath : { } pdp
                }
            } vm && !string.IsNullOrEmpty(pdp))
        {
            return await vm.SaveEntity(ModelTypeEnum.SpeditionNote);
        }

        return false;
    }

    // Menu Navigation
    private void OnPrintNavClick(object sender, RoutedEventArgs e)
    {
        // Show Print Invoice view, hide Capture view
        PrintNoteView.IsVisible = true;
        CaptureNewDetailView.IsVisible = false;

        // Update button styles
        BtnPrinting.Classes.Add("active");
        BtnCapturing.Classes.Remove("active");
    }

    private void OnCaptureNavClick(object sender, RoutedEventArgs e)
    {
        // Show Capture Invoice view, hide Print view
        PrintNoteView.IsVisible = false;
        CaptureNewDetailView.IsVisible = true;

        // Update button styles
        BtnCapturing.Classes.Add("active");
        BtnPrinting.Classes.Remove("active");
    }

    private async void OnPrintPreviewClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    PrintableExpeditionNote :
                    {
                        SelectedReceipt: { } receipt,
                        SelectedTruck: { } truck,
                        SelectedMaterial: { } material,
                        SelectedDriver: { } driver,
                    }
                } model)
            {
                var reportPrepared = await model.PrepareReport();
                if (reportPrepared)
                {
                    WindowState = WindowState.Minimized;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = model.PrintableExpeditionNote.PdfDocumentPath,
                        UseShellExecute = true
                    });
                }

                // Process.Start(new ProcessStartInfo
                // {
                //     FileName = pdfFileName,
                //     UseShellExecute = true,
                //     CreateNoWindow = true,
                //     Verb = "print",
                //     WindowStyle = ProcessWindowStyle.Hidden
                // });
            }

            // Process.Start(new ProcessStartInfo
            // {
            //     FileName = @"C:\\Program Files (x86)\\Foxit Software\\Foxit PDF Reader\\FoxitPDFReader.exe",  // Main Foxit Reader executable
            //     Arguments = $"/p \"D:\\Development\\AvalonXpeditionNoteGenerator\\AvalonXpeditionNoteGenerator\\ReportTemplate\\innosys.pdf\"",  // /p = print command
            //     UseShellExecute = false,
            //     CreateNoWindow = true
            // }); 
        }
        catch (Exception ex)
        {
           Log.Error(ex, "Error while preparing report preview.");
        }
        finally
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsLoading = false;
            }
        }
    }

    private async void OnPrintClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    PrintableExpeditionNote :
                    {
                        SelectedReceipt: { } receipt,
                        SelectedTruck: { } truck,
                        SelectedMaterial: { } material,
                        SelectedDriver: { } driver
                    }
                } model)
            {
                model.IsLoading = true;
                var result = await model.PrepareReport();

                // Process.Start(new ProcessStartInfo
                // {
                //     FileName = pdfFileName,
                //     UseShellExecute = true,
                //     CreateNoWindow = true,
                //     Verb = "print",
                //     WindowStyle = ProcessWindowStyle.Hidden
                // });

                if (result)
                {
                    model.IsLoading = false;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = App.Settings.PathToPdfViewer, // Main Foxit Reader executable
                        Arguments =
                            $"/t \"{model.PrintableExpeditionNote.PdfDocumentPath}\"", // /p = print command /t = print and close
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while printing report.");
            await MessageDialog.Show(this, SystemError, ex.ToString());
        }
        finally
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsLoading = false;
            }
        }
    }

    private async void OnNewDocument(object sender, RoutedEventArgs e)
    {
        try
        {
            await InsertNewNoteNumber();

            DataContext = await LoadData();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    // Capture Note View Actions
    private async void OnAddReceiptClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        ReceiptCode: { Length: > 0 } receiptCode
                    } newReceipt,
                } model && !string.IsNullOrWhiteSpace(receiptCode))
            {
                bool res = await model.SaveEntity(ModelTypeEnum.Receipt);

                ShowMessage(res ? OK : SystemError,
                    res
                        ? $"Рецепта '{newReceipt.ReceiptCode}' запазена успешно."
                        : $"Рецепта '{newReceipt.ReceiptCode}' не е запазена.");
                newReceipt.ReceiptCode = string.Empty;
                await model.RefreshReceiptDataSource();
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterReceipt);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while adding receipt.");
        }
    }

    private async void OnAddClientClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        ClientName: { Length: > 0 } clientName
                    } newClient,
                } model && !string.IsNullOrWhiteSpace(clientName))
            {
                bool res = await model.SaveEntity(ModelTypeEnum.Client);

                ShowMessage(res ? OK : SystemError,
                    res
                        ? $"Клиент/Обект '{newClient.ClientName}' запазен успешно."
                        : $"Клиент '{newClient.ClientName}' не е запазен.");
                newClient.ClientName = string.Empty;
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterClient);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, "Error while adding client {@ex}", ex);
        }
    }

    private async void OnAddDriverClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        DriverName: { Length: > 0 } driverName
                    } newDriver,
                } model && !string.IsNullOrWhiteSpace(driverName))
            {
                bool res = await model.SaveEntity(ModelTypeEnum.Driver);
                this.DataContext = await LoadData();
                ShowMessage(res ? OK : SystemError,
                    res
                        ? $"Шофьор '{newDriver.DriverName}' запазен успешно."
                        : $"Шофьор '{newDriver.DriverName}' не е запазен.");

                newDriver.DriverName = string.Empty;
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterDriver);
            }
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error while adding driver.");
        }
    }


    private async void OnAddMaterialClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        MaterialCode: { Length: > 0 } materialCode
                    } newMaterial,
                } model && !string.IsNullOrWhiteSpace(materialCode))
            {
                bool res = await model.SaveEntity(ModelTypeEnum.Material);
                DataContext = await LoadData();
                ShowMessage(res ? OK : SystemError,
                    res
                        ? $"Материал '{newMaterial.MaterialCode}' запазен успешно."
                        : $"Материал '{newMaterial.MaterialCode}' не е запазен.");
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterMaterial);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while adding material.");
        }
    }

    private async void OnAddTruckClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        TruckClass: { Length: > 0 } truckClass,
                        TruckRegNumber: { Length: > 0 } truckRegNumber
                    } newTruck,
                } model && !string.IsNullOrWhiteSpace(truckClass) && !string.IsNullOrWhiteSpace(truckRegNumber))
            {
                bool res = await model.SaveEntity(ModelTypeEnum.Truck);
                DataContext = await LoadData();
                ShowMessage(res ? OK : SystemError,
                    res
                        ? $" {newTruck.TruckClass}, регистрация {newTruck.TruckRegNumber} запазен успешно."
                        : $" {newTruck.TruckClass}, регистрация {newTruck.TruckRegNumber} не е запазен.");
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterTruckClassAndRegNumber);
            }

        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error while adding truck.");
        }
    }

    // Helper method to show messages
    private async void ShowMessage(string title, string message)
    {
        try
        {
            await MessageDialog.Show(this, title, message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnNumericUpDownKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        // Move focus to next control or parent
        if (sender is Control control)
        {
            CheckWater.Focus();
        }
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            // Cancel closing temporarily
            e.Cancel = true;

            await InsertNewNoteNumber();
            Log.Error(exception, "Error while closing application.");
        }
        finally
        {
            Closing -= OnWindowClosing;
            await Log.CloseAndFlushAsync();  
            Close();
        }
    }
}