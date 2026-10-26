using DriveNow.Domain.Services;

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

    [Theory]
    [MemberData(nameof(Dates))]
    public void CalculateAge_BeforeBirthday_ReturnsAgeMinusOne(DateTime dateOfBirth, DateTime onDate, int expected)
    {
        // arange + act
        var age = RentalMath.CalculateAge(dateOfBirth, onDate);

        // assert
        Assert.Equal(expected, age);
    }
}
