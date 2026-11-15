using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Volvan.Data;

namespace Volvan.Views;

public partial class MainView : UserControl
{
    private Client? _selectedClient;
    private Car? _selectedCar;
    private Rental? _selectedRental;

    public MainView()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        var clients = db.Clients.ToList();
        var cars = db.Cars.ToList();

        ClientsListBox.ItemsSource = clients;
        CarsListBox.ItemsSource = cars;

        if (RentalClientComboBox != null) RentalClientComboBox.ItemsSource = clients;
        if (RentalCarComboBox != null) RentalCarComboBox.ItemsSource = cars;

        RentalsListBox.ItemsSource = db.Rentals
            .Include(r => r.Client)
            .Include(r => r.Car)
            .ToList();
    }

    private void ClientsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ClientsListBox.SelectedItem is not Client client) return;
        _selectedClient = client;
        ClientNameTextBox.Text = client.FullName;
        ClientPhoneTextBox.Text = client.PhoneNumber;
        ClientMessageTextBlock.Text = string.Empty;
    }

    private void CarsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CarsListBox.SelectedItem is not Car car) return;
        _selectedCar = car;
        CarModelTextBox.Text = car.BrandAndModel;
        CarRateTextBox.Text = car.MinuteRate.ToString();
        CarMessageTextBlock.Text = string.Empty;
    }

    private void RentalsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RentalsListBox.SelectedItem is not Rental rental) return;
        _selectedRental = rental;
        RentalMessageTextBlock.Text = string.Empty;
    }

    private bool ValidateClientInput(out string name, out string normalizedPhone, out string altPhone)
    {
        name = ClientNameTextBox.Text?.Trim() ?? string.Empty;
        string phone = ClientPhoneTextBox.Text?.Trim() ?? string.Empty;
        normalizedPhone = string.Empty;
        altPhone = string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            ClientMessageTextBlock.Text = "Введите имя клиента";
            return false;
        }

        if (phone.StartsWith("8") && phone.Length == 11)
        {
            normalizedPhone = "+7" + phone.Substring(1);
            altPhone = "8" + phone.Substring(1);
        }
        else if (phone.StartsWith("+7") && phone.Length == 12)
        {
            normalizedPhone = phone;
            altPhone = "8" + phone.Substring(2);
        }
        else
        {
            ClientMessageTextBlock.Text = "Неверный формат";
            return false;
        }

        return true;
    }

    private void AddClient_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClientMessageTextBlock.Text = string.Empty;
        if (!ValidateClientInput(out string name, out string normalizedPhone, out string altPhone)) return;

        try
        {
            using var db = new AppDbContext();
            if (db.Clients.Any(c => c.PhoneNumber == normalizedPhone || c.PhoneNumber == altPhone))
            {
                ClientMessageTextBlock.Text = "Клиент с таким номером телефона уже существует";
                return;
            }

            db.Clients.Add(new Client { FullName = name, PhoneNumber = normalizedPhone });
            db.SaveChanges();
            LoadData();

            ClientNameTextBox.Text = string.Empty;
            ClientPhoneTextBox.Text = string.Empty;
            _selectedClient = null;

            ClientMessageTextBlock.Text = "Клиент успешно добавлен";
        }
        catch (Exception ex)
        {
            ClientMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void SaveClient_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClientMessageTextBlock.Text = string.Empty;
        if (_selectedClient == null)
        {
            ClientMessageTextBlock.Text = "Выберите клиента для сохранения";
            return;
        }

        if (!ValidateClientInput(out string name, out string normalizedPhone, out string altPhone)) return;

        try
        {
            using var db = new AppDbContext();
            if (db.Clients.Any(c => (c.PhoneNumber == normalizedPhone || c.PhoneNumber == altPhone) && c.Id != _selectedClient.Id))
            {
                ClientMessageTextBlock.Text = "Клиент с таким номером телефона уже существует";
                return;
            }

            var client = db.Clients.Find(_selectedClient.Id);
            if (client != null)
            {
                client.FullName = name;
                client.PhoneNumber = normalizedPhone;
                db.SaveChanges();
                LoadData();
                ClientNameTextBox.Text = string.Empty;
                ClientPhoneTextBox.Text = string.Empty;
                _selectedClient = null;
                ClientMessageTextBlock.Text = "Изменения сохранены";
            }
        }
        catch (Exception ex)
        {
            ClientMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void DeleteClient_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ClientMessageTextBlock.Text = string.Empty;
        if (_selectedClient == null)
        {
            ClientMessageTextBlock.Text = "Выберите клиента для удаления";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var client = db.Clients.Find(_selectedClient.Id);
            if (client != null)
            {
                db.Clients.Remove(client);
                db.SaveChanges();
                LoadData();
                ClientNameTextBox.Text = string.Empty;
                ClientPhoneTextBox.Text = string.Empty;
                _selectedClient = null;
                ClientMessageTextBlock.Text = "Клиент удален";
            }
        }
        catch (Exception ex)
        {
            ClientMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private bool ValidateCarInput(out string model, out int rate)
    {
        model = CarModelTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(model))
        {
            CarMessageTextBlock.Text = "Заполните модель автомобиля";
            rate = 0;
            return false;
        }

        if (!model.All(c => char.IsLetterOrDigit(c) || c == '-' || c == ' '))
        {
            CarMessageTextBlock.Text = "Модель может содержать только буквы, цифры, пробелы и тире";
            rate = 0;
            return false;
        }

        if (model.All(char.IsDigit) || model.All(c => c == '-' || c == ' '))
        {
            CarMessageTextBlock.Text = "Название модели не может состоять только из цифр или знаков тире";
            rate = 0;
            return false;
        }

        string rateStr = CarRateTextBox.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(rateStr, out rate) || rate <= 0)
        {
            CarMessageTextBlock.Text = "Минутный тариф должен быть положительным числом";
            return false;
        }

        return true;
    }

    private void AddCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CarMessageTextBlock.Text = string.Empty;
        if (!ValidateCarInput(out string model, out int rate)) return;

        try
        {
            using var db = new AppDbContext();
            db.Cars.Add(new Car { BrandAndModel = model, MinuteRate = rate });
            db.SaveChanges();
            LoadData();
            CarModelTextBox.Text = string.Empty;
            CarRateTextBox.Text = string.Empty;
            _selectedCar = null;
            CarMessageTextBlock.Text = "Автомобиль добавлен";
        }
        catch (Exception ex)
        {
            CarMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void SaveCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CarMessageTextBlock.Text = string.Empty;
        if (_selectedCar == null)
        {
            CarMessageTextBlock.Text = "Выберите автомобиль для сохранения";
            return;
        }

        if (!ValidateCarInput(out string model, out int rate)) return;

        try
        {
            using var db = new AppDbContext();
            var car = db.Cars.Find(_selectedCar.Id);
            if (car != null)
            {
                car.BrandAndModel = model;
                car.MinuteRate = rate;
                db.SaveChanges();
                LoadData();
                CarModelTextBox.Text = string.Empty;
                CarRateTextBox.Text = string.Empty;
                _selectedCar = null;
                CarMessageTextBlock.Text = "Изменения сохранены";
            }
        }
        catch (Exception ex)
        {
            CarMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void DeleteCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CarMessageTextBlock.Text = string.Empty;
        if (_selectedCar == null)
        {
            CarMessageTextBlock.Text = "Выберите автомобиль для удаления";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var car = db.Cars.Find(_selectedCar.Id);
            if (car != null)
            {
                db.Cars.Remove(car);
                db.SaveChanges();
                LoadData();
                CarModelTextBox.Text = string.Empty;
                CarRateTextBox.Text = string.Empty;
                _selectedCar = null;
                CarMessageTextBlock.Text = "Автомобиль удален";
            }
        }
        catch (Exception ex)
        {
            CarMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void AddRental_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RentalMessageTextBlock.Text = string.Empty;
        if (RentalClientComboBox.SelectedItem is not Client client ||
            RentalCarComboBox.SelectedItem is not Car car ||
            !RentalDatePicker.SelectedDate.HasValue)
        {
            RentalMessageTextBlock.Text = "Выберите клиента, автомобиль и дату";
            return;
        }

        if (RentalDatePicker.SelectedDate.Value.Date < DateTime.Today)
        {
            RentalMessageTextBlock.Text = "Дата не может быть в прошлом";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = new Rental
            {
                ClientId = client.Id,
                CarId = car.Id,
                StartDate = RentalDatePicker.SelectedDate.Value
            };
            db.Rentals.Add(rental);
            db.SaveChanges();
            LoadData();
            RentalMessageTextBlock.Text = "Аренда успешно добавлена";
        }
        catch (Exception ex)
        {
            RentalMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void SaveRental_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RentalMessageTextBlock.Text = string.Empty;
        if (_selectedRental == null)
        {
            RentalMessageTextBlock.Text = "Выберите аренду для сохранения";
            return;
        }

        if (RentalClientComboBox.SelectedItem is not Client client ||
            RentalCarComboBox.SelectedItem is not Car car ||
            !RentalDatePicker.SelectedDate.HasValue)
        {
            RentalMessageTextBlock.Text = "Выберите клиента, автомобиль и дату";
            return;
        }

        if (RentalDatePicker.SelectedDate.Value.Date < DateTime.Today)
        {
            RentalMessageTextBlock.Text = "Дата не может быть в прошлом";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(_selectedRental.Id);
            if (rental != null)
            {
                rental.ClientId = client.Id;
                rental.CarId = car.Id;
                rental.StartDate = RentalDatePicker.SelectedDate.Value;
                db.SaveChanges();
                LoadData();
                RentalMessageTextBlock.Text = "Изменения сохранены";
            }
        }
        catch (Exception ex)
        {
            RentalMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }

    private void DeleteRental_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RentalMessageTextBlock.Text = string.Empty;
        if (_selectedRental == null)
        {
            RentalMessageTextBlock.Text = "Выберите аренду для удаления";
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var rental = db.Rentals.Find(_selectedRental.Id);
            if (rental != null)
            {
                db.Rentals.Remove(rental);
                db.SaveChanges();
                LoadData();
                RentalMessageTextBlock.Text = "Аренда удалена";
            }
        }
        catch (Exception ex)
        {
            RentalMessageTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }
}