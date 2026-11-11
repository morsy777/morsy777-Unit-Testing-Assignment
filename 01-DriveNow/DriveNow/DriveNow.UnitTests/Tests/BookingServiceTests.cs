using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Bogus.DataSets;
using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.External;
using DriveNow.Infrastructure.Persistence;
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
    public static TheoryData<VehicleCategory, InsurancePlan, LoyaltyTier, decimal> DepositScenarios => new()
    {
        { VehicleCategory.Economy, InsurancePlan.Basic, LoyaltyTier.None, 1000m },
        { VehicleCategory.Compact, InsurancePlan.Basic, LoyaltyTier.None, 1000m },
        { VehicleCategory.SUV, InsurancePlan.Basic, LoyaltyTier.None, 2500m },
        { VehicleCategory.Van, InsurancePlan.Basic, LoyaltyTier.None, 2500m },
        { VehicleCategory.Luxury, InsurancePlan.Basic, LoyaltyTier.None, 5000m },
        { VehicleCategory.Economy, InsurancePlan.Premium, LoyaltyTier.None, 500m }
    };

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

        var customerRepo = new Mock<ICustomerRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();

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

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();

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


    [Theory]
    [InlineData(1)]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenPickupBefore2HoursFromNow(int hours)
    {
        // Arrange
        var now = new DateTime(2026, 11, 7, 10, 0, 0);
        var request = new BookingRequest();
        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var promoRepo = new Mock<IPromoCodeRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway
            .Setup(gateway => gateway.Charge(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .Returns("PAY-TEST");

        request.CustomerId = 4;
        request.VehicleId = 2;
        request.PickupAt = now.AddHours(hours);

        var customer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2025, 1, 1)
        };

        var vehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        };

        customerRepo.Setup(repo => repo.GetById(4)).Returns(customer);
        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(vehicle);

        var sut = new BookingService(
            new FakeTimeProvider(now),
            promoRepo.Object,
            customerRepo.Object,
            vehicleRepo.Object,
            bookingRepo.Object,
            paymentGateway.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => sut.CreateBooking(request));

        ex.Message.ShouldBe("Pickup must be at least 2 hours from now");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenRentalPeriodIsInvalid(int rentalDays)
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(rentalDays)
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2025, 1, 1)
        };

        var validVehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(validCustomer);
        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(validVehicle);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object, 
            vehicleRepository: vehicleRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));

        ex.Message.ShouldBe("Rental period is invalid");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenAdditionalDriversOutsideAllowedRange(int additionalDrivers)
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            AdditionalDrivers = additionalDrivers
        };

        var validCustomer = new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2025, 1, 1)
        };

        var validVehicle = new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(validCustomer);
        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(validVehicle);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));

        ex.Message.ShouldBe("Up to 3 additional drivers allowed");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenVehicleAlreadyBookedForRequestedDates()
    {
        // Arrange
        var existingBooking = new Booking
        {
            Id = 100,
            CustomerId = 5,
            VehicleId = 2,
            PickupAt = new DateTime(2026, 11, 10, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0),
            Status = BookingStatus.Reserved
        };

        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = new DateTime(2026, 11, 11, 10, 0, 0),
            ReturnAt = new DateTime(2026, 11, 13, 10, 0, 0)
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2025, 1, 1)
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        });

        bookingRepo
            .Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([existingBooking]);
        bookingRepo
            .Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));

        ex.Message.ShouldBe("Vehicle is already booked for these dates");
    }

    [Fact]
    public void CreateBooking_ShouldThrowBookingRejectedException_WhenCustomerHasTwoActiveBookings()
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1)
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(2004, 7, 1),
            LicenseIssueDate = new DateTime(2025, 1, 1)
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy
        });

        bookingRepo
            .Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo
            .Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(2);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object);

        // Act & Assert
        var ex = Should.Throw<BookingRejectedException>(() => bookingService.CreateBooking(request));

        ex.Message.ShouldBe("Customer already has 2 active bookings");
    }

    [Fact]
    public void CreateBooking_ShouldCalculateCorrectPrice_WhenRequestIsValid()
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(2),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0,
            PromoCode = null
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m
        });

        bookingRepo
            .Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo
            .Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object);

        // Act
        var booking = bookingService.CreateBooking(request);

        // Assert
        booking.Price.RentalDays.ShouldBe(2);
        booking.Price.BaseAmount.ShouldBe(2000m);
        booking.Price.Total.ShouldBe(2000m);
    }

    [Fact]
    public void CreateBooking_ShouldSetReservedStatusCreatedAtAndStartMileage_WhenRequestIsValid()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(
            2026, 11, 9, 10, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(fixedTime);

        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            InsurancePlan = InsurancePlan.Basic,
            AdditionalDrivers = 0
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var vehicleMileage = 2004;

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m,
            Mileage = vehicleMileage
        });

        bookingRepo.Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo.Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object,
            timeProvider: fakeTimeProvider);

        // Act
        var booking = bookingService.CreateBooking(request);

        // Assert
        booking.Status.ShouldBe(BookingStatus.Reserved);
        booking.CreatedAt.ShouldBe(fixedTime.DateTime);
        booking.StartMileage.ShouldBe(vehicleMileage);
    }

    [Fact]
    public void CreateBooking_ShouldGenerateCorrectReference_WhenRequestIsValid()
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            InsurancePlan = InsurancePlan.Basic
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m
        });

        bookingRepo.Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo.Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object);

        // Act
        var booking = bookingService.CreateBooking(request);

        // Assert
        booking.Reference.ShouldMatch("^DN-20261110-[A-Z0-9]{4}$");
    }

    [Theory]
    [MemberData(nameof(DepositScenarios))]
    public void CreateBooking_WithDeposit_ShouldCalculateAndChargeDeposit(VehicleCategory category, InsurancePlan insurancePlan,
        LoyaltyTier loyaltyTier, decimal expectedDeposit)
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            InsurancePlan = insurancePlan
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var paymentGateway = new Mock<IPaymentGateway>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = loyaltyTier
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = category,
            DailyRate = 1000m
        });

        bookingRepo.Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo.Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        paymentGateway
            .Setup(gateway => gateway.Charge(4, expectedDeposit, It.IsAny<string>()))
            .Returns("PAY-TEST");

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object,
            paymentGateway: paymentGateway);

        // Act
        var booking = bookingService.CreateBooking(request);

        // Assert
        booking.Deposit.ShouldBe(expectedDeposit);

        paymentGateway.Verify(gateway => gateway.Charge(
            4,
            expectedDeposit,
            It.Is<string>(description => description == $"Deposit {booking.Reference}")),
            Times.Once);
    }

    [Fact]
    public void CreateBooking_ShouldNotChargeGateway_WhenCustomerIsPlatinum()
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            InsurancePlan = InsurancePlan.Premium
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var paymentGateway = new Mock<IPaymentGateway>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.Platinum
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m
        });

        bookingRepo.Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo.Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object,
            paymentGateway: paymentGateway);

        // Act
        var booking = bookingService.CreateBooking(request);

        // Assert
        booking.Deposit.ShouldBe(0m);

        paymentGateway
            .Verify(gateway => gateway.Charge(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CreateBooking_ShouldNotSaveBooking_WhenPaymentGatewayFails()
    {
        // Arrange
        var pickupAt = new DateTime(2026, 11, 10, 10, 0, 0);
        var request = new BookingRequest
        {
            CustomerId = 4,
            VehicleId = 2,
            PickupAt = pickupAt,
            ReturnAt = pickupAt.AddDays(1),
            InsurancePlan = InsurancePlan.Basic
        };

        var customerRepo = new Mock<ICustomerRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var bookingRepo = new Mock<IBookingRepository>();
        var paymentGateway = new Mock<IPaymentGateway>();

        customerRepo.Setup(repo => repo.GetById(4)).Returns(new Customer
        {
            Id = 4,
            DateOfBirth = new DateTime(1995, 1, 1),
            LicenseIssueDate = new DateTime(2015, 1, 1),
            LoyaltyTier = LoyaltyTier.None
        });

        vehicleRepo.Setup(repo => repo.GetById(2)).Returns(new Vehicle
        {
            Id = 2,
            IsAvailable = true,
            Category = VehicleCategory.Economy,
            DailyRate = 1000m
        });

        bookingRepo.Setup(repo => repo.GetOpenBookingsForVehicle(2))
            .Returns([]);

        bookingRepo.Setup(repo => repo.CountOpenBookingsForCustomer(4))
            .Returns(0);

        paymentGateway
            .Setup(gateway => gateway.Charge(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
            .Throws(new PaymentFailedException("Gateway timeout"));

        var bookingService = CreateBookingService(
            customerRepository: customerRepo.Object,
            vehicleRepository: vehicleRepo.Object,
            bookingRepository: bookingRepo.Object,
            paymentGateway: paymentGateway);

        // Act & Assert
        Should.Throw<PaymentFailedException>(() => bookingService.CreateBooking(request));
        bookingRepo.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Never);
    }



    private static BookingService CreateBookingService(DateTimeOffset? currentTime = null,
        ICustomerRepository? customerRepository = null,
        IVehicleRepository? vehicleRepository = null,
        IBookingRepository? bookingRepository = null,
        FakeTimeProvider? timeProvider = null,
        Mock<IPaymentGateway>? paymentGateway = null)
    {
        timeProvider ??= new FakeTimeProvider(currentTime ??
            new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));

        var promoCodeRepository = new PromoCodeRepository();

        customerRepository ??= new CustomerStub();
        vehicleRepository ??= new VehicleStub();
        bookingRepository ??= new BookingStub();
        if (paymentGateway is null)
        {
            paymentGateway = new Mock<IPaymentGateway>();
            paymentGateway
                .Setup(gateway => gateway.Charge(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns("PAY-TEST");
        }

        return new BookingService(
            timeProvider,
            promoCodeRepository,
            customerRepository,
            vehicleRepository,
            bookingRepository,
            paymentGateway.Object);
    }
}
