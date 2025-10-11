namespace AvalonXpeditionNoteGenerator.Models;

public class Truck
{
    public int Id
    {
        get; set;
    }

    public string TruckType
    {
        get; set;
    } = string.Empty;

    public string TruckRegNo
    {
        get; set;
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