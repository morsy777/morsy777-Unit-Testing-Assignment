using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Domain.ValueObjects;
using DriveNow.Infrastructure.External;
using DriveNow.Infrastructure.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;

namespace DriveNow.UnitTests.Tests;

public class ReturnServiceTests
{
    [Fact]
    public void ProcessReturn_ShouldRejectNonActiveBooking_WhenBookingIsNotActive()
    {
        var booking = CreateBooking(BookingStatus.Completed);
        var service = CreateService(booking);

        var exception = Should.Throw<InvalidBookingStateException>(() =>
            service.ProcessReturn(booking.Id, booking.StartMileage, 4, 0));

        exception.Message.ShouldBe("Only active bookings can be returned");
    }

    [Fact]
    public void ProcessReturn_ShouldRejectEndMileageBelowStartMileage_WhenEndMileageIsTooLow()
    {
        var booking = CreateBooking();
        var service = CreateService(booking);

        var exception = Should.Throw<ArgumentException>(() =>
            service.ProcessReturn(booking.Id, booking.StartMileage - 1, 4, 0));

        exception.Message.ShouldBe("End mileage cannot be below the start mileage");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void ProcessReturn_ShouldRejectFuelQuartersOutsideRange_WhenFuelQuartersAreInvalid(int fuelQuarters)
    {
        var booking = CreateBooking();
        var service = CreateService(booking);

        var exception = Should.Throw<ArgumentException>(() =>
            service.ProcessReturn(booking.Id, booking.StartMileage, fuelQuarters, 0));

        exception.Message.ShouldBe("Fuel quarters must be between 0 and 4");
    }

    [Fact]
    public void ProcessReturn_ShouldUseCurrentTimeAndNotRefundUnusedRentalDays_WhenReturnIsEarly()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);

        var booking = CreateBooking();
        booking.ReturnAt = now.DateTime.AddDays(2);
        booking.Price = new PriceBreakdown
        {
            RentalDays = 3,
            Total = 300m
        };
        booking.Deposit = 100m;

        var payments = new Mock<IPaymentGateway>();

        var service = CreateService(booking, now, payments);

        var report = service.ProcessReturn(booking.Id, booking.StartMileage, 4, 0);

        booking.ActualReturnAt.ShouldBe(now.UtcDateTime);

        report.AllowedKm.ShouldBe(750);

        payments.Verify(payment => payment.Refund(
            booking.CustomerId,
            booking.Deposit,
            $"Deposit refund {booking.Reference}"), Times.Once);

        payments.Invocations
            .Count(invocation => invocation.Method.Name == nameof(IPaymentGateway.Refund))
            .ShouldBe(1);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(59, 59, 0)]
    [InlineData(60, 60, 10)]
    [InlineData(61, 61, 20)]
    [InlineData(240, 240, 40)]
    [InlineData(241, 241, 100)]
    [InlineData(1441, 1441, 200)] // 1440 is no. of hours in day.
    public void ProcessReturn_ShouldCalculateLateFee_WhenReturnMinutesVary(int minutesAfterPlannedReturn, int expectedLateMinutes, decimal expectedLateFee)
    {
        var now = new DateTimeOffset(2026, 11, 10, 10, 0, 0, TimeSpan.Zero);

        var booking = CreateBooking();
        booking.ReturnAt = now.DateTime.AddMinutes(-minutesAfterPlannedReturn);
        booking.Price.RentalDays = 1;

        var service = CreateService(booking, now);

        var report = service.ProcessReturn(booking.Id, booking.StartMileage, 4, 0);

        report.LateMinutes.ShouldBe(expectedLateMinutes);
        report.LateFee.ShouldBe(expectedLateFee);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, 2, 500, 0, 0)]
    [InlineData(VehicleCategory.Economy, 2, 510, 10, 7.50)]
    [InlineData(VehicleCategory.Luxury, 2, 510, 10, 15.00)]
    public void ProcessReturn_ShouldCalculateMileageFee_WhenDistanceExceedsAllowance(VehicleCategory category, int rentalDays, int drivenKilometres,
        int expectedExtraKilometres, decimal expectedMileageFee)
    {
        var booking = CreateBooking();
        booking.Price.RentalDays = rentalDays;
        booking.ReturnAt = new DateTime(2026, 11, 12, 10, 0, 0);

        var service = CreateService(booking, vehicleCategory: category);

        var report = service.ProcessReturn(booking.Id, booking.StartMileage + drivenKilometres, 4, 0);

        report.AllowedKm.ShouldBe(250 * rentalDays);
        report.ExtraKm.ShouldBe(expectedExtraKilometres);
        report.MileageFee.ShouldBe(expectedMileageFee);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, 4, 0, 0)]
    [InlineData(VehicleCategory.Economy, 3, 1, 200)]
    [InlineData(VehicleCategory.Economy, 2, 2, 400)]
    [InlineData(VehicleCategory.SUV, 3, 1, 300)]
    [InlineData(VehicleCategory.Van, 2, 2, 600)]
    public void ProcessReturn_ShouldCalculateFuelFee_WhenFuelQuartersAreMissing(VehicleCategory category, int fuelQuarters,
        int expectedMissingQuarters, decimal expectedFuelFee)
    {
        var booking = CreateBooking();
        var service = CreateService(booking, vehicleCategory: category);

        var report = service.ProcessReturn(booking.Id, booking.StartMileage, fuelQuarters, 0);

        report.MissingFuelQuarters.ShouldBe(expectedMissingQuarters);
        report.FuelFee.ShouldBe(expectedFuelFee);
    }

    private static ReturnService CreateService(Booking booking, DateTimeOffset? now = null,
        Mock<IPaymentGateway>? payments = null, VehicleCategory vehicleCategory = VehicleCategory.Economy)
    {
        var bookings = new Mock<IBookingRepository>();
        bookings.Setup(repository => repository.GetById(booking.Id))
            .Returns(booking);

        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(repository => repository.GetById(booking.VehicleId))
            .Returns(new Vehicle
            {
                Id = booking.VehicleId,
                Category = vehicleCategory,
                DailyRate = 100m,
                Mileage = booking.StartMileage
            });

        var customers = new Mock<ICustomerRepository>();
        customers.Setup(repository => repository.GetById(booking.CustomerId))
            .Returns(new Customer
            {
                Id = booking.CustomerId,
                Email = "customer@example.com"
            });

        return new ReturnService(
            new FakeTimeProvider(now ?? new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero)),
            bookings.Object,
            vehicles.Object,
            customers.Object,
            (payments ?? new Mock<IPaymentGateway>()).Object,
            Mock.Of<IEmailSender>());
    }

    private static Booking CreateBooking(BookingStatus status = BookingStatus.Active) =>
        new()
        {
            Id = 1,
            Reference = "BK-001",
            CustomerId = 2,
            VehicleId = 3,
            StartMileage = 1000,
            PickupAt = new DateTime(2026, 11, 8, 9, 0, 0),
            ReturnAt = new DateTime(2026, 11, 9, 9, 0, 0),
            Status = status,
            InsurancePlan = InsurancePlan.Premium,

            Price = new PriceBreakdown { RentalDays = 1, Total = 100m }
        };
}
