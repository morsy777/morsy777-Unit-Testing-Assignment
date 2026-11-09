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
            PickupAt = DateTime.Today.AddDays(1),
            ReturnAt = DateTime.Today.AddDays(3),
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
            PickupAt = DateTime.Today.AddDays(1),
            ReturnAt = DateTime.Today.AddDays(3),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var blacklistedCustomer = new Customer
        {
            Id = 1,
            FullName = "Mohamed Morsi",
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2022, 1, 1),
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

    private static BookingService CreateBookingService(DateTimeOffset? currentTime = null, ICustomerRepo? customerRepository = null)
    {
        var timeProvider = new FakeTimeProvider(currentTime ??
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        var promoCodeRepository = new PromoCodeRepository();

        customerRepository ??= new CustomerStub();

        return new BookingService(timeProvider, promoCodeRepository, customerRepository);
    }
}
