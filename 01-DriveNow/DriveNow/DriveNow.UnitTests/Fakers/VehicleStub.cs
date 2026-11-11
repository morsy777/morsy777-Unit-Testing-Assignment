using DriveNow.Domain.Entities;
using DriveNow.Infrastructure.Repositories;
using DriveNow.UnitTests.Tests;

namespace DriveNow.UnitTests.Fakers;

public class VehicleStub : IVehicleRepository
{
    public List<Vehicle> GetAll()
    {
        return [];
    }

    public Vehicle? GetById(int id)
    {
        return null;
    }

    public void Update(Vehicle vehicle)
    {

    }
}