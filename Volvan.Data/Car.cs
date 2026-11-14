namespace Volvan.Data;

public class Car
{
    public int Id { get; set; }
    public string BrandAndModel { get; set; } = string.Empty;
    public int ChildSeatCount { get; set; }
    public int MinuteRate { get; set; }
    public bool IsAvailable { get; set; }
    public List<Rental> Rentals { get; set; } = [];

    public override string ToString() => $"{BrandAndModel}, {MinuteRate} руб/мин";
}