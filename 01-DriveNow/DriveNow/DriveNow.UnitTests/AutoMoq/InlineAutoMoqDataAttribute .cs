using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Time.Testing;

namespace DriveNow.UnitTests.AutoMoq;

public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(CreateFixture(), values)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize(new AutoMoqCustomization());
        fixture.Inject<TimeProvider>(
            new FakeTimeProvider(
                new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero)));
        return fixture;
    }
}
