using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

public sealed class VehicleFaker : Faker<Vehicle>
{
    public VehicleFaker()
    {
        RuleFor(v => v.PlateNumber,
            f => f.Random.Replace("???-####"));

        RuleFor(v => v.Model, f => f.Vehicle.Model());
        RuleFor(v => v.Category,
            f => f.PickRandom<VehicleCategory>());

        RuleFor(v => v.DailyRate,
            f => f.Random.Decimal(500, 10000));

        RuleFor(v => v.Mileage, f => f.Random.Int(0, 200000));
        RuleFor(v => v.IsAvailable, f => f.Random.Bool());

        UseSeed(2004);
    }

    public VehicleFaker WithDailyRate(decimal dailyRate)
    {
        RuleFor(v => v.DailyRate, _ => dailyRate);

        return this;
    }

    public VehicleFaker WithCategory(VehicleCategory category)
    {
        RuleFor(v => v.Category, _ => category);

        return this;
    }
}
