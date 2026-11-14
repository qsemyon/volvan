namespace Volvan.Data;
public class Rental
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public DateTime StartDate { get; set; }
    public int TotalCost { get; set; }

    public override string ToString() => $"Аренда #{Id}: {Client?.FullName} взял {Car?.BrandAndModel}";
}