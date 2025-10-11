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
using static AvalonXpeditionNoteGenerator.Constants.Constants;

namespace AvalonXpeditionNoteGenerator;

public partial class MainWindow : Window
{
        public  MainWindow()
        {
            InitializeComponent();
            
            Loaded += async (sender, e) =>
            {
                this.DataContext = await LoadData();
            };
            
        }

        private async Task<MainWindowViewModel> LoadData() => await MainWindowViewModel.LoadData();
        
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
        
        private  async void OnPrintPreviewClick(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (DataContext is MainWindowViewModel
                    {
                        PrintableExpeditionNote :
                        {
                            SelectedReceipt: { } receipt ,
                            SelectedTruck: { }  truck ,
                            SelectedMaterial: {}  material ,
                            SelectedDriver: {}  driver ,
                            
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

                        await model.SaveEntity(ModelTypeEnum.SpeditionNote);
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
                Console.WriteLine(ex.ToString());
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
                            SelectedReceipt: { } receipt ,
                            SelectedTruck: { }  truck ,
                            SelectedMaterial: {}  material ,
                            SelectedDriver: {}  driver 
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
                            await model.SaveEntity(ModelTypeEnum.SpeditionNote);
                            model.IsLoading = false;
                            
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = App.Settings.PathToPdfViewer, // Main Foxit Reader executable
                                Arguments = $"/t \"{model.PrintableExpeditionNote.PdfDocumentPath}\"", // /p = print command /t = print and close
                                UseShellExecute = false,
                                CreateNoWindow = true
                            });
                        }
                }
                
            }
            catch (Exception ex)
            {
                await MessageDialog.Show(this, SystemError,ex.ToString());
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
                DataContext = await LoadData();;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
              
        }
        
        // Capture Note View Actions
        private async void OnAddReceiptClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        ReceiptCode: { Length: > 0  } receiptCode
                    } newReceipt,
                } model && !string.IsNullOrWhiteSpace(receiptCode))
            {
                
                bool res = await model.SaveEntity(ModelTypeEnum.Receipt);
               
                ShowMessage(res ? OK : SystemError, res ? $"Рецепта '{newReceipt.ReceiptCode}' запазена успешно." : 
                                                                      $"Рецепта '{newReceipt.ReceiptCode}' не е запазена." );
                newReceipt.ReceiptCode = string.Empty;
                await model.RefreshDataSource();
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterReceipt);
            }
        }

        private async void OnAddDriverClick(object sender, RoutedEventArgs e)
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


        private async void OnAddMaterialClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        MaterialCode:{Length: > 0  } materialCode
                    } newMaterial,
                }model && !string.IsNullOrWhiteSpace(materialCode))
            {
                
                bool res = await model.SaveEntity(ModelTypeEnum.Material);
                DataContext = await LoadData();
                ShowMessage(res? OK : SystemError, res ? $"Материал '{newMaterial.MaterialCode}' запазен успешно." :
                                                                     $"Материал '{newMaterial.MaterialCode}' не е запазен.");
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterMaterial);
            }
        }
        
        private async void OnAddTruckClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel
                {
                    EntityToAdd:
                    {
                        TruckClass:{Length: >0  } truckClass,
                        TruckRegNumber:{Length:> 0  } truckRegNumber
                    } newTruck,
                } model && !string.IsNullOrWhiteSpace(truckClass) && !string.IsNullOrWhiteSpace(truckRegNumber))
            {
               
                bool res = await model.SaveEntity(ModelTypeEnum.Truck);
                DataContext = await LoadData();
                ShowMessage(res ? OK: SystemError, res? $" {newTruck.TruckClass}, регистрация {newTruck.TruckRegNumber} запазен успешно.":
                                                                    $" {newTruck.TruckClass}, регистрация {newTruck.TruckRegNumber} не е запазен.");
            }
            else
            {
                ShowMessage(InvalidDataMessage, EnterTruckClassAndRegNumber);
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
}