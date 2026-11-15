using DriveNow.Domain.Entities;

namespace DriveNow.Infrastructure.Repositories;

public interface IBookingRepository
{
    Booking? GetById(int id);
    List<Booking> GetAll();
    List<Booking> GetOpenBookingsForVehicle(int vehicleId);
    int CountOpenBookingsForCustomer(int customerId);
    void Add(Booking booking);
    void Update(Booking booking);
}
