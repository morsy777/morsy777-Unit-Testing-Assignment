# DriveNow Naming Conventions

## 1. Test Projects, Classes, and Files

- **Test Projects:** Test project names must end with `.Tests`, e.g., `DriveNow.Tests`.
- **Test Classes:** Test class names must end with `Tests`, e.g., `BookingServiceTests`.
- **Test Files:** Each test file must have the same name as its corresponding test class and use the `.cs` extension, e.g., `BookingServiceTests.cs`.
- **Test Class Organization:** Each test class must primarily test one production class.

## 2. Test Method Naming

### 2.1 Compared Naming Styles

Three common test method naming styles were considered:

1. **Method_Scenario_ExpectedResult**
   - Example: `BookCar_WhenAvailable_ReturnsBooking`
   - Clearly identifies the method, scenario, and expected result.

2. **Should_X_When_Y**
   - Example: `Should_ReturnBooking_WhenCarIsAvailable`
   - Expresses the expected behavior in a readable sentence.

3. **Given_When_Then**
   - Example: `GivenAvailableCar_WhenBooking_ThenReturnsBooking`
   - Separates the initial conditions, action, and expected outcome.

### 2.2 Selected Style

The selected style is `Method_Scenario_ExpectedResult`.

This style is easy to scan, explicitly identifies the method under test, and makes the scenario and expected result clear. It also provides a consistent structure across the test suite.

### 2.3 DriveNow Examples

The following examples illustrate the selected naming style:

- `BookCar_WhenCarIsAvailable_ReturnsBooking`
- `CancelBooking_WhenBookingIsAlreadyCancelled_ReturnsFalse`
- `CalculatePrice_WhenDiscountApplies_ReturnsDiscountedPrice`

These examples must be adjusted to match the actual methods and business rules in DriveNow.

**Rule:** Every test method name must follow the `Method_Scenario_ExpectedResult` structure, using underscores to separate the three parts.

## 3. Test Doubles and Test Data

### 3.1 Hand-written Test Doubles

- **Fakes:** Classes that provide working test implementations of dependencies must use the `Fake` prefix, e.g., `FakeBookingRepository`.
- **Spies:** Classes that record interactions with dependencies must use the `Spy` prefix, e.g., `SpySmsSender`.
- **Stubs:** Classes that provide predefined responses must use the `Stub` prefix, e.g., `StubPaymentGateway`.
- Test double names must include the name of the dependency they replace or observe.
- Hand-written test doubles must be placed in the test project, not in the production project.

### 3.2 Bogus Fakers

- Bogus faker classes must end with `Faker`, e.g., `BookingFaker`.
- Each faker must generate data for one main model or entity.
- Faker classes must be placed in a dedicated test-data folder or namespace.

### 3.3 AutoFixture Customizations

- AutoFixture customization classes must end with `Customization`, e.g., `BookingCustomization`.
- Each customization must implement `ICustomization`.
- Customizations must be placed in the test project under a dedicated customizations folder or namespace.

### 3.4 AutoFixture Attributes

- Custom AutoFixture data attributes must end with `AutoDataAttribute`, e.g., `AutoMoqDataAttribute`.
- Each custom attribute must derive from the appropriate AutoFixture attribute or data-attribute type required by the test framework.
- Custom attributes must be placed in the test project under a dedicated attributes folder or namespace.

**Rule:** These naming conventions apply when the corresponding test doubles or test-data helpers are used in the project.

## 4. Production Code

### 4.1 Interfaces

- Interface names must start with `I`, followed by a PascalCase noun or descriptive name, e.g., `IBookingRepository`.
- Interface files must have the same name as the interface, e.g., `IBookingRepository.cs`.
- Interfaces must be placed in the namespace associated with their architectural responsibility.

### 4.2 Repositories

- Repository implementation names must end with `Repository`, e.g., `BookingRepository`.
- Repository interfaces must end with `Repository` and start with `I`, e.g., `IBookingRepository`.
- Repository files must have the same name as their corresponding classes.
- Repository implementations must be placed in the infrastructure or data-access namespace.

### 4.3 Services

- Service implementation names must end with `Service`, e.g., `BookingService`.
- Service interface names must start with `I` and end with `Service`, e.g., `IBookingService`.
- Service files must have the same name as their corresponding classes or interfaces.
- Application business services must be placed in the application or services namespace.

### 4.4 Exceptions

- Custom exception class names must end with `Exception`, e.g., `BookingNotFoundException`.
- Exception files must have the same name as their corresponding exception classes.
- Custom exceptions must be placed in the namespace of the layer that owns the exception, such as the domain or application layer.

### 4.5 Namespaces

The following namespace conventions must be followed, subject to the existing DriveNow architecture:

- **Application services and service interfaces:** `DriveNow.Application.Services`
- **Domain repository interfaces:** `DriveNow.Domain.Repositories`
- **Infrastructure repository implementations:** `DriveNow.Infrastructure.Repositories`
- **Domain exceptions:** `DriveNow.Domain.Exceptions`
- **Test classes and test helpers:** Namespaces must reflect the test project and their folder organization.

All namespaces must use PascalCase and must match the project's established root namespace and architectural structure.

### 4.6 File and Type Naming

- Class, interface, enum, and record names must use PascalCase.
- Each production source file must use the name of its primary public type.
- Repository and service implementations must not be placed in the test project.
- Production interfaces, repositories, services, and exceptions must be placed in the appropriate architectural layer.

## 5. Enforcement

- Naming rules must be specific enough to verify through automated tests or static analysis.
- Tests enforcing these rules must use the conventions defined in this document.
- If a rule does not apply because a particular type or tool is not used in DriveNow, that rule must be documented as not applicable rather than treated as a violation.
- Any changes to these conventions must be reflected in this document and in the tests that enforce them.