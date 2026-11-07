using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DriveNow.UnitTests.Fakers;

public class CustomerFaker : Faker<Customer>
{
    public CustomerFaker()
    {
        RuleFor(c => c.FullName, f => f.Name.FullName());
        RuleFor(c => c.Email, f => f.Internet.Email());
        RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("010########"));

        RuleFor(c => c.DateOfBirth,
            f => f.Date.Between(
                new DateTime(1970, 1, 1),
                new DateTime(2005, 1, 1)));

        RuleFor(c => c.LicenseIssueDate,
            f => f.Date.Between(
                new DateTime(2010, 1, 1),
                new DateTime(2025, 1, 1)));

        RuleFor(c => c.LoyaltyTier,
            f => f.PickRandom<LoyaltyTier>());

        RuleFor(c => c.IsBlacklisted, false);
        RuleFor(c => c.CompletedRentals, f => f.Random.Int(0, 50));

        UseSeed(2004);
    }

    public CustomerFaker WithAge(int age, DateTime? referenceDate = null)
    {
        var date = referenceDate ?? new DateTime(2026, 11, 7);

        RuleFor(c => c.DateOfBirth, _ => date.AddYears(-age));

        return this;
    }

    public CustomerFaker WithTier(LoyaltyTier tier)
    {
        RuleFor(c => c.LoyaltyTier, _ => tier);

        return this;
    }
}
