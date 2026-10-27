using DriveNow.Application.Models;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Services;
using DriveNow.Infrastructure.Persistence;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTestsData
{
    // arrange
    Customer customer;
    Vehicle vehicle;
    DateTime pickupAt = DateTime.Now.AddMonths(1);
    DateTime returnedAt = DateTime.Now.AddMonths(1).AddDays(10);

    public BookingRequest CreateBookingDependencies()
    {
        return new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickupAt,
            ReturnAt = returnedAt,
            AdditionalDrivers = 0,
            PromoCode = default
        };
    }

    public BookingServiceTestsData(bool platinumOnly = false, bool isAvailable = true)
    {
        vehicle = InMemoryDatabase.Vehicles.FirstOrDefault(x => x.IsAvailable == isAvailable)!;
        int licenseYears = vehicle.Category != VehicleCategory.Van ? 1 : 2;
        licenseYears = vehicle.Category != VehicleCategory.Luxury ? licenseYears : 3;

        int age = vehicle.Category != VehicleCategory.Luxury ? 21 : 26;

        customer = InMemoryDatabase.Customers.FirstOrDefault(x => !x.IsBlacklisted  
            && RentalMath.CalculateAge(x.DateOfBirth, pickupAt) >= age
            && RentalMath.CalculateLicenseYears(x.LicenseIssueDate, pickupAt) >= licenseYears
            && (!platinumOnly || x.LoyaltyTier == LoyaltyTier.Platinum))!;
    }
}
