# Hidden Inputs Audit

The following table identifies hidden inputs in the application, explains how they affect tests, and documents the proposed approach for removing them.

> **Note:** Exact line numbers should be added from the source files. The removal approaches below must be updated to reflect the changes actually implemented.

| Class · Method                 | Line | Hidden Input                                                              | Why It Hurts a Test                                                                                                                     | How You Removed It                                                                                                                          |
| ------------------------------ | ---- | ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `BookingService.CreateBooking` | TODO | `DateTime.Now` in the rental start validation                             | The result depends on the current system time. The same booking can pass or fail depending on when the test runs.                       | Inject `TimeProvider` and use a controlled time in tests with `FakeTimeProvider`.                                                           |
| `BookingService.CreateBooking` | TODO | `DateTime.Now` in the pickup validation                                   | The two-hour minimum pickup rule depends on the current time, making boundary tests unreliable.                                         | Inject `TimeProvider` and use a fixed time to test pickup times before, exactly at, and after the two-hour boundary.                        |
| `BookingService.CreateBooking` | TODO | Random code generation in `RandomCode()`                                  | Generated values can differ between runs, making tests unpredictable if they depend on specific generated codes.                        | I Will test the number of digits.                                                                                                           |
| `CancellationService.Cancel`   | TODO | `DateTime.Now` in the booking-start validation                            | Whether cancellation is allowed depends on the time the test executes, potentially producing inconsistent results near the pickup time. | Inject `TimeProvider` and control the current time in tests using `FakeTimeProvider`.                                                       |
| `CancellationService.Cancel`   | TODO | `DateTime.Now` in the cancellation fee calculation                        | The calculated number of hours and cancellation fee can vary depending on when the method runs.                                         | Inject `TimeProvider` and capture the current time once per operation, then use that same value for the calculation.                        |
| `PricingService.PromoDiscount` | TODO | `DateTime.Today` in the promotion validity check                          | Promotion eligibility depends on the machine's current date, so tests can behave differently on different days or at date boundaries.   | Inject `TimeProvider` and derive the current date from the provider, allowing tests to control the date and verify the validity boundaries. |
| `ReturnService.ProcessReturn`  | TODO | `DateTime.Now` used to build the report and record the actual return time | The recorded return timestamp changes between test runs, making exact timestamp assertions difficult without a controlled clock.        | Inject `TimeProvider`, obtain the current time once, and reuse it for both `BuildReport` and `ActualReturnAt`.                              |

## Testing Strategy

- Use `FakeTimeProvider` to provide a fixed time for time-dependent business rules.
- Test boundary values explicitly, including times immediately before, exactly at, and immediately after each relevant threshold.
- Use a deterministic test implementation of the code generator to avoid relying on random values.
- Keep each operation's timestamp consistent by reading the clock once when multiple calculations or assignments must use the same time.
- Replace each `TODO` with the actual source line number after the implementation is complete.

