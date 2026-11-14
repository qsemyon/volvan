namespace Volvan.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Clients.Any()) return;

        var client1 = new Client { FullName = "Иванов Иван Иванович", DriverExperienceYears = 5, IsVerified = true, PhoneNumber = "+79241112233" };
        var client2 = new Client { FullName = "Петров Петр Петрович", DriverExperienceYears = 2, IsVerified = true, PhoneNumber = "+79244445566" };
        var client3 = new Client { FullName = "Смирнов Алексей Сергеевич", DriverExperienceYears = 8, IsVerified = true, PhoneNumber = "+79247778899" };

        var car1 = new Car { BrandAndModel = "Toyota Rav4", ChildSeatCount = 1, MinuteRate = 20, IsAvailable = true };
        var car2 = new Car { BrandAndModel = "Hyundai Tucson", ChildSeatCount = 0, MinuteRate = 18, IsAvailable = true };
        var car3 = new Car { BrandAndModel = "Toyota Camry", ChildSeatCount = 0, MinuteRate = 15, IsAvailable = false };

        db.Clients.AddRange(client1, client2, client3);
        db.Cars.AddRange(car1, car2, car3);
        db.SaveChanges();

        db.Rentals.AddRange(
            new Rental { ClientId = client1.Id, CarId = car1.Id, StartDate = DateTime.Now.AddDays(-2), TotalCost = 1500 },
            new Rental { ClientId = client2.Id, CarId = car2.Id, StartDate = DateTime.Now.AddDays(-1), TotalCost = 900 }
        );
        db.SaveChanges();
    }
}