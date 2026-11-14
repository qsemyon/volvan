using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Volvan.Data;

namespace Volvan.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        DbSeed.Seed(db);

        DataContext = new
        {
            Clients = db.Clients.ToList(),
            Cars = db.Cars.ToList(),
            Rentals = db.Rentals.Include(r => r.Client).Include(r => r.Car).ToList()
        };
    }
}