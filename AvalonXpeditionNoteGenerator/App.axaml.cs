using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvalonXpeditionNoteGenerator.Config;
using AvalonXpeditionNoteGenerator.Data;
using Microsoft.Extensions.Configuration;

namespace AvalonXpeditionNoteGenerator;

public partial class App : Application
{
    
    private static IConfiguration Configuration { get;  set; } = null!;
    public static AppSettings Settings { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        LoadConfiguration();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeDatabase();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void InitializeDatabase()
    {
        try
        {
            using var db = new AppDbContext(AppConfig.DatabasePath);
            
            // Create database and apply migrations
            db.Database.EnsureCreated();
            
            // Or use migrations (better for production):
            // db.Database.Migrate();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
      
    }
    
    private void LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true);
        
        Configuration = builder.Build();
        Settings = new AppSettings();
        // Bind to strongly-typed object
        Configuration.Bind(Settings);
    }
}