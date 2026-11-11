using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Repositories;

namespace DriveNow.UnitTests.Fakers;

public class BookingStub : IBookingRepository
{
    public void Add(Booking booking)
    {
        return;
    }

    public int CountOpenBookingsForCustomer(int customerId)
    {
        return 0;
    }

    public Booking? GetById(int id)
    {
        return null;
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId)
    {
        return [];
    }

    public void Update(Booking booking)
    {
        return;
    }
}
