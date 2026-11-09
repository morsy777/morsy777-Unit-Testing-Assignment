using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.Repositories;
using DriveNow.UnitTests.Fakers;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;

namespace DriveNow.UnitTests.Tests;

public class BookingServiceTests
{
    [Fact]
    public void CreatBooking_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 999, // Any value will return null from the stub
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var bookingService = CreateBookingService();

        // Act & Assert
        var ex = Should.Throw<CustomerNotFoundException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe($"Customer #999 not found");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenCustomerBlacklisted()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 1, // This customer is blacklisted in the stub
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var blacklistedCustomer = new Customer
        {
            Id = 1,
            IsBlacklisted = true
        };

        var customerRepo = new Mock<ICustomerRepo>();

        customerRepo
            .Setup(repo => repo.GetById(1))
            .Returns(blacklistedCustomer);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("Customer is blacklisted");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenCustomerAgeUnder21()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 2, // This customer is under 21 in the stub
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var underageCustomer = new Customer
        {
            Id = 2,
            DateOfBirth = new DateTime(2006, 1, 1), // Under 21 years old
        };

        var customerRepo = new Mock<ICustomerRepo>();

        customerRepo
            .Setup(repo => repo.GetById(2))
            .Returns(underageCustomer);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("Driver must be at least 21");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenLicenseYearsUnder1()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 3, // This customer has less than 1 year of license in the stub
            VehicleId = 1,
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var inexperiencedCustomer = new Customer
        {
            Id = 3,
            LicenseIssueDate = new DateTime(2025, 12, 1), // Less than 1 year of license
        };

        var customerRepo = new Mock<ICustomerRepo>();

        customerRepo
            .Setup(repo => repo.GetById(3))
            .Returns(inexperiencedCustomer);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("License must be held for at least 1 year");
    }



    private static BookingService CreateBookingService(DateTimeOffset? currentTime = null,
        ICustomerRepo? customerRepository = null, IVehicleRepo? vehicleRepository = null)
    {
        var timeProvider = new FakeTimeProvider(currentTime ??
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        var promoCodeRepository = new PromoCodeRepository();

        customerRepository ??= new CustomerStub();
        vehicleRepository ??= new VehicleStub();

        return new BookingService(timeProvider, promoCodeRepository, customerRepository, vehicleRepository);
    }
}
