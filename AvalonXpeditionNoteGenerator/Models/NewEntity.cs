using CommunityToolkit.Mvvm.ComponentModel;

namespace AvalonXpeditionNoteGenerator.Models;

public partial class NewEntity : ObservableObject
{
    [ObservableProperty]
    private string receiptCode = string.Empty;
    
    [ObservableProperty]
    private string driverName = string.Empty;
    
    [ObservableProperty]
    private string truckClass = string.Empty;
    
    [ObservableProperty]
    private string truckRegNumber = string.Empty;
    
    [ObservableProperty]
    private string materialCode = string.Empty;
    
    [ObservableProperty]
    private string clientName = string.Empty;
    
    public static NewEntity Create() => new();
}