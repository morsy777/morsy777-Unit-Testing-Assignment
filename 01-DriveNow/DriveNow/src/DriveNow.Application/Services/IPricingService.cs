using DriveNow.Application.Models;
using DriveNow.Domain.Entities;
using DriveNow.Domain.ValueObjects;

namespace DriveNow.Application.Services;

public interface IPricingService
{
    PriceBreakdown Calculate(Customer customer, Vehicle vehicle, BookingRequest request);
}
