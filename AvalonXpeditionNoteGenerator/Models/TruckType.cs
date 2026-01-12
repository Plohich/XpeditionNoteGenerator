using System.ComponentModel.DataAnnotations.Schema;

namespace AvalonXpeditionNoteGenerator.Models;

public class Truck
{
    public int Id
    {
        get; init;
    }
    
    [Column(TypeName = "nvarchar(150)")]
    public string TruckType
    {
        get; init;
    } = string.Empty;
    
    [Column(TypeName = "nvarchar(10)")]
    public string TruckRegNo
    {
        get; init;
    } = string.Empty;
    public static Truck Create(int id, string truckType, string truckRegNo) => new ()
    {
        Id = id,
        TruckType = truckType,
        TruckRegNo = truckRegNo
    };
    public static Truck CreateWithTypeAndReg(string truckType, string truckRegNo) => new ()
    {
        TruckType = truckType,
        TruckRegNo = truckRegNo
    };
    
    public string TruckDescription => $"{TruckType} - {TruckRegNo}";
};