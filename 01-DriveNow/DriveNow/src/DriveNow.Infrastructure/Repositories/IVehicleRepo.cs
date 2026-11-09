using DriveNow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.Infrastructure.Repositories;

public interface IVehicleRepo
{
    Vehicle? GetById(int id);
    List<Vehicle> GetAll();
    void Update(Vehicle vehicle);
}
