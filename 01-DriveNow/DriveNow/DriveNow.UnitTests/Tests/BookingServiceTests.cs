using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Bogus.DataSets;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.Repositories;
using DriveNow.UnitTests.AutoMoq;
using DriveNow.UnitTests.Fakers;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using Vehicle = DriveNow.Domain.Entities.Vehicle;

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

    [Fact]
    public void CreateBooking_ShouldThrowVehicleNotFoundException_WhenVehicleDoesNotExist()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 4, 
            VehicleId = 999, // Any value will return null from the stub
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1), // Over 21 years old
            LicenseIssueDate = new DateTime(2015, 1, 1), // More than 1 year of license
        };

        var customerRepo = new Mock<ICustomerRepo>();

        customerRepo
            .Setup(repo => repo.GetById(4))
            .Returns(validCustomer);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object);

        // Act & Assert
        var ex = Should.Throw<VehicleNotFoundException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe($"Vehicle #999 not found");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenVehicleNotAvailable()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 4, // This customer is valid in the stub
            VehicleId = 2, // This vehicle is not available in the stub
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 10, 10, 0, 0),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1), // Over 21 years old
            LicenseIssueDate = new DateTime(2015, 1, 1), // More than 1 year of license
        };

        var unavailableVehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = false // Vehicle is not available
        };

        var customerRepo = new Mock<ICustomerRepo>();
        var vehicleRepo = new Mock<IVehicleRepo>();

        customerRepo
            .Setup(repo => repo.GetById(4))
            .Returns(validCustomer);

        vehicleRepo
            .Setup(repo => repo.GetById(2))
            .Returns(unavailableVehicle);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object, vehicleRepository: vehicleRepo.Object);
        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("Vehicle is not available");
    }

    // I spent a lot of time trying to get AutoFixture to work with the BookingService and its dependencies,
    // but I couldn't get it to work properly. I tried using InlineAutoMoqDataAttribute, but it didn't seem to
    // inject the dependencies correctly. I also tried using AutoDataAttribute, but it didn't work either.
    // I think the problem is that the BookingService has a lot of dependencies, and AutoFixture doesn't know how to create them all.
    // I think it would be better to just use manual mocks for the dependencies, as I did in the other tests.
    //[Theory]
    //[InlineAutoMoqData(20)]
    //[InlineAutoMoqData(21)]
    //[InlineAutoMoqData(25)]

    //public void CreateBooking_ShouldThrowBookingRejectedException_WhenCustomerAgeUnder21UsingAutoFixture(
    //    int age,
    //    BookingRequest request,
    //    [Frozen] Mock<ICustomerRepo> customerRepo,
    //    BookingService sut)
    //{
    //    // arrange
    //    request.PickupAt = DateTime.Today.AddDays(2);
    //    request.ReturnAt = request.PickupAt.AddDays(2);

    //    var customer = new Customer
    //    {
    //        Id = request.CustomerId,
    //        DateOfBirth = DateTime.Today.AddYears(-age),
    //        IsBlacklisted = false,
    //        LicenseIssueDate = DateTime.Today.AddYears(-5)
    //    };

    //    customerRepo.Setup(repo => repo.GetById(request.CustomerId)).Returns(customer);

    //    // act & assert
    //    var ex = Should.Throw<BookingRejectedException>(() => sut.CreateBooking(request));
    //    ex.Message.ShouldBe("Driver must be at least 21");
    //}

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenLuxturyVeichleAndAgeUnder23AndLicenseUnder3Years()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 4, // This customer is valid in the stub
            VehicleId = 2, // This vehicle is not available in the stub
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1), // Over 21 years old
            LicenseIssueDate = new DateTime(2025, 1, 1), // 1 year of license
        };

        var validVehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Luxury
        };

        var customerRepo = new Mock<ICustomerRepo>();
        var vehicleRepo = new Mock<IVehicleRepo>();

        customerRepo
            .Setup(repo => repo.GetById(4))
            .Returns(validCustomer);

        vehicleRepo
            .Setup(repo => repo.GetById(2))
            .Returns(validVehicle);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object, vehicleRepository: vehicleRepo.Object);
        
        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("Luxury vehicles require age 25+ and 3 years of license");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenVanAndLicenseUnder2Years()
    {
        // Arrange
        var request = new BookingRequest
        {
            CustomerId = 4, // This customer is valid in the stub
            VehicleId = 2, // This vehicle is not available in the stub
            PickupAt = new DateTime(2026, 11, 8, 10, 0, 0),
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1), // Over 21 years old
            LicenseIssueDate = new DateTime(2025, 1, 1), // 1 year of license
        };

        var validVehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Van
        };

        var customerRepo = new Mock<ICustomerRepo>();
        var vehicleRepo = new Mock<IVehicleRepo>();

        customerRepo
            .Setup(repo => repo.GetById(4))
            .Returns(validCustomer);

        vehicleRepo
            .Setup(repo => repo.GetById(2))
            .Returns(validVehicle);

        var bookingService = CreateBookingService(customerRepository: customerRepo.Object, vehicleRepository: vehicleRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));
        ex.Message.ShouldBe("Vans require 2 years of license");
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
