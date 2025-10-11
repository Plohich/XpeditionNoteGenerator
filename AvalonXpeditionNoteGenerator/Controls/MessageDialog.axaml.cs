using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace AvalonXpeditionNoteGenerator.Controls;

public partial class MessageDialog : Window
{
    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<MessageDialog, string>(nameof(Message));
        
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
        
    public ICommand CloseCommand { get; }
        
    private MessageDialog()
    {
        InitializeComponent();
        CloseCommand = new RelayCommand(Close);
        DataContext = this;
    }
        
    public static async Task Show(Window parent, string title, string message)
    {
        var dialog = new MessageDialog()
        {
            Title = title,
            Message = message,
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };
            
        await dialog.ShowDialog(parent);
    }
}