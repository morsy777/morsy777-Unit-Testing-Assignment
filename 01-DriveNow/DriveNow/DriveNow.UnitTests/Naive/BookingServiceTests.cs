using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using Shouldly;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTests
{
    [Fact]
    public void CreateBooking_HappyPathe_ReturnsBooking()
    {
        var service = new BookingService();
        var request = new BookingServiceTestsData().CreateBookingDependencies();
        var result = service.CreateBooking(request);

        result.ShouldBeOfType<Booking>();

        result.CustomerId.ShouldBe(request.CustomerId);
        result.VehicleId.ShouldBe(request.VehicleId);
        result.Status.ShouldBe(BookingStatus.Reserved);
    }

    [Fact]
    public void CreateBooking_PlatinumLoyality_ReturnsPlatinumBooking()
    {
        var service = new BookingService();
        var request = new BookingServiceTestsData(platinumOnly: true)
            .CreateBookingDependencies();

        var result = service.CreateBooking(request);

        result.ShouldBeOfType<Booking>();

        result.Deposit.ShouldBe(0m);
    }

    [Fact]
    public void CreateBooking_BoockedViechle_ThrowsBookingRejectedException()
    {
        var service = new BookingService();
        var request = new BookingServiceTestsData(isAvailable: false)
            .CreateBookingDependencies();

        Should.Throw<BookingRejectedException>(() => service.CreateBooking(request));
    }
}

