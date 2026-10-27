# FIRST Audit — Naive Tests

## F — Fast

**Violated.**

`CreateBooking_HappyPathe_ReturnsBooking` takes about 2 seconds to run. This is slow for a unit test.

## I — Independent

**Violated.**

All three tests pass when I run them separately. However, when I run them together, only `CreateBooking_HappyPathe_ReturnsBooking` fails.

The tests share the same in-memory database. Changes made by other tests can affect the happy path test.

## R — Repeatable

**Potentially violated.**

The test data uses `DateTime.Now`. The results may change depending on when the tests run.

The tests also share in-memory data, which can be changed by other tests.

## S — Self-validating

**Not violated.**

The tests use Shouldly assertions to check the results. They pass or fail automatically without checking the results manually.

## T — Timely

**Potentially violated.**

`BookingService` creates its dependencies directly using `new`. This makes it difficult to replace real dependencies with test doubles and to test the service in isolation.

## Test Run Results

| Run | Time | Failed Tests |
|---|---|---|
| 1 | 524 ms | `CreateBooking_HappyPathe_ReturnsBooking` |
| 2 | 528 ms | `CreateBooking_HappyPathe_ReturnsBooking` |
| 3 | 531 ms | `CreateBooking_HappyPathe_ReturnsBooking` |
| 4 | 530 ms | `CreateBooking_HappyPathe_ReturnsBooking` |
| 5 | 528 ms | `CreateBooking_HappyPathe_ReturnsBooking` |

## Production Code Problems

- **Slow test:** `CreateBooking` calls real services such as payment, SMS, and email services.
- **Shared state:** `BookingService` creates its own repositories, which use the shared in-memory database.
- **Time dependency:** `RejectVehicle` uses `DateTime.Now` to validate the pickup time.
- **Hard to isolate:** `BookingService` creates its dependencies directly using `new`.

The exact production lines should be added after checking the source code.