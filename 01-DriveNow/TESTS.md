# Feature: [Feature Name]

## 1. Business Rule Coverage

| Business Rule                    | Test Name(s)       | Status  |
| -------------------------------- | ------------------ | ------- |
| BR1: [Business rule description] | `Actual_Test_Name` | Covered |
| BR2: [Business rule description] | `Actual_Test_Name` | Covered |
| BR3: [Business rule description] | None               | Gap     |

Every identified business rule is mapped to the test cases that verify it. Any rule without a test is explicitly marked as a gap.

## 2. Boundary Values Tested

| Business Rule | Boundary Values  | Expected Behavior   | Test Name          |
| ------------- | ---------------- | ------------------- | ------------------ |
| BR1           | [Below boundary] | [Expected behavior] | `Actual_Test_Name` |
| BR1           | [Exact boundary] | [Expected behavior] | `Actual_Test_Name` |
| BR1           | [Above boundary] | [Expected behavior] | `Actual_Test_Name` |

The boundary values are selected based on the business rules of this feature.

## 3. AI-Suggested Test Cases

| AI Suggestion              | Decision | Why                    |
| -------------------------- | -------- | ---------------------- |
| [AI-suggested test case 1] | Accepted | [Reason for accepting] |
| [AI-suggested test case 2] | Rejected | [Reason for rejecting] |
| [AI-suggested test case 3] | Accepted | [Reason for accepting] |

Only accepted AI suggestions are implemented. Accepted cases are committed separately from the other changes.

## 4. Verdict

I think these tests are enough because [explain which business rules, boundaries, and edge cases are covered].

OR:

I know I am missing [list the business rules or edge cases that are not covered yet].
