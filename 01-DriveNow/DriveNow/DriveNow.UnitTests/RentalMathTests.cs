using DriveNow.Domain.Services;

namespace DriveNow.UnitTests;

public class RentalMathTests
{
    // Happy: DataOfBirth 2000-01-01, OnDate 2024-02-02 then it 24 years old

    // Boundary(Leap year): DataOfBirth 2000-02-29, OnDate 2024-02-29 -> BD(02/29) then it 24 years old, years++
    // Boundary(Leap year): DataOfBirth 2000-02-29, OnDate 2024-02-28 -> BD(02/29) then it 23 years old, years--
    // Boundary(Regular year): DataOfBirth 2000-02-29 OnDate 2025-02-28 -> BD(02/28) then it 25 years old, years++
    // Boundary(Regular year): DataOfBirth 2000-02-29 OnDate 2025-02-27 -> BD(02/28) then it 24 years old, years--

    public static TheoryData<DateTime, DateTime, int> Dates => new()
    {
        { new DateTime(2000, 2, 29), new DateTime(2024, 2, 29), 24 },
        { new DateTime(2000, 2, 29, 0, 0, 0), new DateTime(2024, 2, 28), 23 },
        { new DateTime(2000, 2, 29, 0, 0, 0), new DateTime(2024, 2, 28, 23, 59, 59), 23 },
        { new DateTime(2000, 2, 29, 0, 0, 0), new DateTime(2025, 2, 28), 25 },
        { new DateTime(2000, 2, 29, 0, 0, 0), new DateTime(2025, 2, 27), 24 },
        { new DateTime(2000, 2, 29, 0, 0, 0), new DateTime(2025, 2, 27,23, 59, 59), 24 }
    };

    [Theory]
    [MemberData(nameof(Dates))]
    public void CalculateAge_IsLeapYear_ReturnDayOfBirthDecreasedByOne(DateTime dateOfBirth, DateTime onDate, int expected)
    {
        // arange + act
        var age = RentalMath.CalculateAge(dateOfBirth, onDate);

        // assert
        Assert.Equal(expected, age);
    }
}
