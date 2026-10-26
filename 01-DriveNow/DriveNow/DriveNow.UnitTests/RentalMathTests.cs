using DriveNow.Domain.Services;
using FluentAssertions;

namespace DriveNow.UnitTests;

public class RentalMathTests
{
    public static TheoryData<DateTime, DateTime, int> Dates => new()
    {
        { new DateTime(2000, 2, 29), new DateTime(2024, 2, 29), 24 },
        { new DateTime(2000, 2, 29), new DateTime(2024, 2, 28), 23 },

        { new DateTime(2000, 2, 29), new DateTime(2025, 2, 28), 25 },
        { new DateTime(2000, 2, 29), new DateTime(2025, 2, 27), 24 },

        { new DateTime(2004, 7, 1), new DateTime(2026, 7, 1), 22 },
        { new DateTime(2004, 7, 1), new DateTime(2026, 6, 30), 21 },
    };

    public static TheoryData<DateTime, DateTime, int> RentalDates => new()
    {
        { new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2024, 2, 2, 0, 0, 0), 1 },
        { new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2024, 2, 2, 0, 59, 0), 1 },
        { new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2024, 2, 2, 1, 0, 0), 1 },
        { new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2024, 2, 2, 1, 1, 0), 2 },
        { new DateTime(2024, 2, 1, 0, 0, 0), new DateTime(2024, 2, 3, 0, 0, 0), 2 },

        { new DateTime(2024, 2, 1), new DateTime(2024, 2, 10), 9 },
    };

    public static TheoryData<DateTime, DateTime> InValidRentalDates => new()
    {
        { new DateTime(2024, 2, 1), new DateTime(2024, 1, 1)},
    };

    [Theory]
    [MemberData(nameof(Dates))]
    public void CalculateAge_BeforeBirthday_ReturnsAgeMinusOne(DateTime dateOfBirth, DateTime onDate, int expected)
    {
        // arange + act
        var age = RentalMath.CalculateAge(dateOfBirth, onDate);

        // assert
        Assert.Equal(expected, age);
    }

    [Theory]
    [MemberData(nameof(Dates))]
    public void CalculateLicenseYears_OnBirthday_ReturnsCorrectYears(DateTime dateOfBirth, DateTime onDate, int expected)
    {
        // arange + act
        var age = RentalMath.CalculateAge(dateOfBirth, onDate);

        // assert
        Assert.Equal(expected, age);
    }

    [Theory]
    [MemberData(nameof(RentalDates))]
    public void CalculateRentalDays_MoreThan60min_ReturnsOneDays(DateTime pickup, DateTime returnAt, int expected)
    {
        // arange + act
        var days = RentalMath.CalculateRentalDays(pickup, returnAt);

        Assert.Equal(expected, days);
    }

    [Theory]
    [MemberData(nameof(InValidRentalDates))]
    public void CalculateRentalDays_ReturnAtBeforePickup_ThrowsArgumentException(DateTime pickup, DateTime returnAt)
    {
        Action act = () => RentalMath.CalculateRentalDays(pickup, returnAt);
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("Return must be after pickup");
    }
}
