using Microsoft.EntityFrameworkCore;
using Volvan.Data;

Console.WriteLine($"Путь к базе данных: {AppDbContext.DbPath}");

using var db = new AppDbContext();
db.Database.EnsureCreated();
DbSeed.Seed(db);

Console.WriteLine("\nКлиенты");
foreach (var client in db.Clients)
{
    Console.WriteLine(client);
}

Console.WriteLine("\nАвтомобили");
foreach (var car in db.Cars)
{
    Console.WriteLine(car);
}

Console.WriteLine("\nАренды");
var rentals = db.Rentals
    .Include(r => r.Client)
    .Include(r => r.Car)
    .ToList();

foreach (var rental in rentals)
{
    Console.WriteLine(rental);
}