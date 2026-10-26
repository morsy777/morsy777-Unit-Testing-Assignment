# Feature: RentalMath

## 1. Business Rule Coverage

| Business Rule                    | Test Name(s)       | Status  |
| -------------------------------- | ------------------ | ------- |
| BR-01 | `CalculateAge_BeforeBirthday_ReturnsAgeMinusOne` | Covered |
| BR-02 | `CalculateLicenseYears_OnBirthday_ReturnsCorrectYears` | Covered |
| BR-03 | `CalculateRentalDays_Befor60min_ReturnsOneDay` | Covered |
| BR-03 | `CalculateRentalDays_NormalCase_ReturnsReminderDays` | Covered |
| BR-03 | `CalculateRentalDays_ReturnAtBeforePickup_ThrowsArgumentException` | Covered |

Every identified business rule is mapped to the test cases that verify it. Any rule without a test is explicitly marked as a gap.

## 2. Boundary Values Tested

| Business Rule | Boundary Values | Expected Behavior | Test Name |
| --- | --- | --- | --- |
| BR-01 | One day before the birthday (leap year) | Returns age minus one | `CalculateAge_BeforeBirthday_ReturnsAgeOneLess` |
| BR-01 | Exact birthday (leap year) | Returns the correct age | `CalculateAge_OnBirthday_ReturnsCorrectAge` |
| BR-01 | One day before February 28 (non-leap year, born on February 29) | Returns age minus one | `CalculateAge_BeforeBirthday_ReturnsAgeOneLess` |
| BR-01 | February 28 in a non-leap year (born on February 29) | Counts February 28 as the birthday | `CalculateAge_OnBirthday_ReturnsCorrectAge` |
| BR-01 | One day before a regular birthday | Returns age minus one | `CalculateAge_BeforeBirthday_ReturnsAgeOneLess` |
| BR-01 | Exact regular birthday | Returns the correct age | `CalculateAge_OnBirthday_ReturnsCorrectAge` |

The boundary values are selected based on the business rules of this feature.

## 3. AI-Suggested Test Cases

| AI Suggestion              | Decision | Why                    |
| -------------------------- | -------- | ---------------------- |
| In BR-01: Test one day before | Accepted | its boundary value |
| In BR-01: Test one day after | rejected | I already test on the same date so I didn't need to test one day after |
| In BR-01: Test with any month other than February and any day other than 29 | Accepted | To ensure it decrease days only for February |
| In In BR-03: | Accepted | it advance me to test in 59 min and in this case the remainder shouldn't increase by one, refactor condition in CalculateRentalDays to satisfy the BR |

Only accepted AI suggestions are implemented. Accepted cases are committed separately from the other changes.

## 4. Verdict

I think these tests are enough because in BR-01 I cover day before, at the same day in the leap and regular year, and in BR-03 I cover case that the method throws, refactor CalculateRentalDays to add additional one day only if the reminder minuties i greater than 60 not just equal.

