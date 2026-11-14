namespace Volvan.Data;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int DriverExperienceYears { get; set; }
    public bool IsVerified { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public List<Rental> Rentals { get; set; } = [];

    public override string ToString() => $"{FullName} ({PhoneNumber})";
}