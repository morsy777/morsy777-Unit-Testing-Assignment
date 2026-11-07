using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Services;
using DriveNow.Infrastructure.Repositories;
using DriveNow.UnitTests.Fakers;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{
    [Theory]
    [InlineData(2026, 11, 5, 0)]   // Thursday
    [InlineData(2026, 11, 6, 200)] // Friday
    [InlineData(2026, 11, 7, 200)] // Saturday
    public void Calculate_WhenRentalIncludesWeekendDays_ApplySurcharge(int year, int month, int day, decimal expectedSurcharge)
    {
        // arrange
        var customer = new CustomerFaker()
            .WithAge(23)
            .WithTier(LoyaltyTier.Silver)
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(1000m)
            .Generate();

        var pickup = new DateTime(year, month, day, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(1),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var sut = CreatePricingService();

        // Act
        var result = sut.Calculate(customer, vehicle, request);

        // Assert
        result.WeekendSurcharge.ShouldBe(expectedSurcharge);
    }

    [Theory]
    [InlineData(20, 0)]
    [InlineData(21, 150)]
    [InlineData(22, 150)]
    [InlineData(23, 150)]
    [InlineData(24, 150)]
    [InlineData(25, 0)]
    public void Calculate_YoungDriverSurchargeShouldApply15Percent_WhenAgeIsBetween21And24(int age, decimal expectedYoungDriverSurcharge)
    {
        // arrange
        var pickup = new DateTime(2026, 11, 5, 10, 0, 0);

        var customer = new CustomerFaker()
            .WithAge(age, pickup)
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(1000m)
            .Generate();

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(1),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var sut = CreatePricingService();

        // Act
        var result = sut.Calculate(customer, vehicle, request);

        // Assert
        RentalMath.CalculateAge(customer.DateOfBirth, pickup).ShouldBe(age);

        result.YoungDriverSurcharge.ShouldBe(expectedYoungDriverSurcharge); // 15% of 1000
    }

    [Theory]
    [InlineData(7, 0)]
    [InlineData(13, 1300)]
    [InlineData(14, 2100)]
    public void Calculate_DurationDiscountShouldApply10Percent_WhenRentalDurationIsBetween7And13Days(int days, decimal expectedDurationDiscount)
    {
        // arrange
        var pickup = new DateTime(2026, 11, 5, 10, 0, 0);
        var customer = new CustomerFaker()
            //.WithAge(30, pickup)
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(1000m)
            .Generate();

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(days),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var sut = CreatePricingService();

        // Act
        var result = sut.Calculate(customer, vehicle, request);

        // Assert
        RentalMath.CalculateRentalDays(request.PickupAt, request.ReturnAt).ShouldBe(days);

        result.DurationDiscount.ShouldBe(expectedDurationDiscount); // 10% of 8000 (8 days * 1000)
    }

    [Theory]
    [InlineData(LoyaltyTier.None, 0)]
    [InlineData(LoyaltyTier.Silver, 50)]
    [InlineData(LoyaltyTier.Gold, 100)]
    [InlineData(LoyaltyTier.Platinum, 150)]
    public void Calculate_DiscountPercentageShouldApplyCorrectPercentage_WhenCustomerHasLoyaltyTier(LoyaltyTier tier, decimal expectedDiscount)
    {
        // arrange
        var pickup = new DateTime(2026, 11, 5, 10, 0, 0);
        var customer = new CustomerFaker()
            .WithTier(tier)
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(1000m)
            .Generate();

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(1),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var sut = CreatePricingService();

        // Act
        var result = sut.Calculate(customer, vehicle, request);

        // Assert
        result.LoyaltyDiscount.ShouldBe(expectedDiscount); // 5% Silver, 10% Gold, 15% Platinum of Base(1000)
    }

    private static PricingService CreatePricingService(DateTimeOffset? currentTime = null)
    {
        var timeProvider = new FakeTimeProvider(currentTime ??
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        var promoCodeRepository = new Mock<IPromoCodeRepository>();

        return new PricingService(timeProvider, promoCodeRepository.Object);
    }
}
