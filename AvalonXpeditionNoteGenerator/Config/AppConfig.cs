using System;
using System.IO;

namespace AvalonXpeditionNoteGenerator.Config;

public static class AppConfig
{
    private static string  appDataPath = string.Empty;
        
    public static string AppDataPath
    {
        get
        {
            if ( string.IsNullOrEmpty(appDataPath) )
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                appDataPath = Path.Combine(localAppData, "AvalonXpeditionNoteGenerator");
                Directory.CreateDirectory(appDataPath);
            }
            return appDataPath;
        }
    }
        
    public static string DatabasePath => Path.Combine(AppDataPath, "database.db");
    public static string PdfPath(string fileName) => Path.Combine(AppDataPath, fileName);
    
    public static string ReportTemplatePath => "ReportTemplate/SpeditionListDoubleBands.frx";
    
}