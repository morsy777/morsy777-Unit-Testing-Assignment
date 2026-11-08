using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.Services;
using DriveNow.Domain.ValueObjects;
using DriveNow.Infrastructure.Repositories;
using DriveNow.UnitTests.Fakers;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{

    public static TheoryData<Customer, Vehicle, BookingRequest, PriceBreakdown>
    FullPricingScenarios => new()
    {
        // Worked Example A
        {
            new Customer
            {
                DateOfBirth = new DateTime(1992, 1, 1),
                LoyaltyTier = LoyaltyTier.Gold
            },
            new Vehicle
            {
                DailyRate = 600m,
                Category = VehicleCategory.Economy
            },
            new BookingRequest
            {
                PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
                ReturnAt = new DateTime(2026, 11, 11, 10, 0, 0),
                InsurancePlan = InsurancePlan.Standard,
                AdditionalDrivers = 0,
                PromoCode = null
            },
            new PriceBreakdown
            {
                RentalDays = 3,
                BaseAmount = 1800m,
                WeekendSurcharge = 0m,
                YoungDriverSurcharge = 0m,
                DurationDiscount = 0m,
                LoyaltyDiscount = 180m,
                PromoDiscount = 0m,
                InsuranceCost = 450m,
                AdditionalDriversCost = 0m,
                Total = 2070m
            }
        },
    
        // Worked Example B
        {
            new Customer
            {
                DateOfBirth = new DateTime(2003, 1, 1),
                LoyaltyTier = LoyaltyTier.Silver
            },
            new Vehicle
            {
                DailyRate = 1500m,
                Category = VehicleCategory.SUV
            },
            new BookingRequest
            {
                PickupAt = new DateTime(2026, 11, 5, 9, 0, 0),
                ReturnAt = new DateTime(2026, 11, 12, 9, 30, 0),
                InsurancePlan = InsurancePlan.Premium,
                AdditionalDrivers = 1,
                PromoCode = "SAVE15"
            },
            new PriceBreakdown
            {
                RentalDays = 7,
                BaseAmount = 10500m,
                WeekendSurcharge = 600m,
                YoungDriverSurcharge = 1575m,
                DurationDiscount = 1050m,
                LoyaltyDiscount = 525m,
                PromoDiscount = 1050m,
                InsuranceCost = 2100m,
                AdditionalDriversCost = 700m,
                Total = 12850m
            }
        }
    };

    [Theory]
    [MemberData(nameof(FullPricingScenarios))]
    public void Calculate_WhenFullPricingScenarioProvided_ReturnsExpectedBreakdown(Customer customer, Vehicle vehicle,
        BookingRequest request, PriceBreakdown expected)
    {
        var sut = CreatePricingService();

        var result = sut.Calculate(customer, vehicle, request);

        result.Should().BeEquivalentTo(expected);
    }

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

    [Theory]
    [InlineData(1000, 1, 150)]
    [InlineData(1500, 2, 450)]
    public void Calculate_PromoCodeDiscountShouldApplyOnBaseAmount_WhenPromoCodeIsValid(decimal dailyRate, int days, decimal expectedDiscount)
    {
        // arrange
        var pickup = new DateTime(2026, 11, 5, 10, 0, 0);
        var customer = new CustomerFaker()
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(dailyRate)
            .Generate();

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(days),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = "SAVE15"
        };

        var sut = CreatePricingService();

        // Act
        var result = sut.Calculate(customer, vehicle, request);

        // Assert
        result.PromoDiscount.ShouldBe(expectedDiscount); // 15% of Base(dailyRate)
    }

    [Theory]
    [InlineData(null, "SAVE1000")]
    [InlineData(null, "SUMMER25")]
    [InlineData(VehicleCategory.Economy, "LUX10")]
    [InlineData(VehicleCategory.Luxury, "LUX10")]
    [InlineData(VehicleCategory.Economy, "WEEKLY20")]
    public void Calculate_ShouldThrowInvalidPromoCodeException_WhenPromoCodeIsInvalid(VehicleCategory? category, string code)
    {
        // arrange
        var pickup = new DateTime(2026, 11, 5, 10, 0, 0);
        var customer = new CustomerFaker()
            .Generate();

        var vehicle = new VehicleFaker()
            .WithDailyRate(1000m)
            .WithCategory(category)
            .Generate();

        var request = new BookingRequest
        {
            CustomerId = customer.Id,
            VehicleId = vehicle.Id,
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(3),
            InsurancePlan = default,
            AdditionalDrivers = 0,
            PromoCode = code
        };

        var sut = CreatePricingService();

        // Act & Assert
        Should.Throw<InvalidPromoCodeException>(() => sut.Calculate(customer, vehicle, request));
    }


    private static PricingService CreatePricingService(DateTimeOffset? currentTime = null)
    {
        var timeProvider = new FakeTimeProvider(currentTime ??
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        var promoCodeRepository = new PromoCodeRepository(); // Use the actual repository implementation for testing

        // If you want to mock the repository instead, you can uncomment the following lines and comment out the above line.
        //var promoCodeRepository = new Mock<IPromoCodeRepository>();
        //var promo = new PromoCode
        //{
        //    Code = "SAVE15",
        //    DiscountPercent = 15,
        //    ValidFrom = DateTime.Today.AddDays(-10),
        //    ValidUntil = DateTime.Today.AddDays(60),
        //    MinDays = 1,
        //    AllowedCategories = null,
        //    MaxUses = 100,
        //    UsedCount = 3
        //};
        //promoCodeRepository.Setup(r => r.GetByCode("SAVE15")).Returns(promo);

        return new PricingService(timeProvider, promoCodeRepository);
    }
}
