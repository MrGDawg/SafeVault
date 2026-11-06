# SafeVault Capstone Project Summary

## Microsoft Security and Authentication Course

### Project Overview

SafeVault is an ASP.NET Core MVC application developed as the Capstone project for the Microsoft **Security and Authentication** course.

The project was developed through three activities:

1. **Secure Coding and Input Validation**
2. **Authentication and Role-Based Authorization**
3. **Security Debugging, Hardening, and Testing**

The objective was to apply secure coding practices to a functioning web application and progressively protect SafeVault against insecure input, SQL injection, cross-site scripting (XSS), unauthorized access, and insecure password handling.

Microsoft Copilot was used throughout the project to assist with code generation, security review, debugging, refactoring, and the creation of automated security tests.

---

Controllers/
    AccountController.cs     Authentication
    AdminController.cs       RBAC-protected functionality

Data/
    SafeVaultDatabase.cs     Parameterized SQLite access

Models/
    UserInputModel.cs        Input validation
    LoginViewModel.cs        Login validation
    UserAccount.cs           Account representation

Services/
    PasswordService.cs       Password hashing/verification
    DevelopmentAccountSeeder.cs

SafeVault.Tests/
    SecurityTests.cs
    AuthorizationIntegrationTests.cs
    SecurityAttackIntegrationTests.cs

# Activity 1: Secure Coding and Input Validation

The first activity established SafeVault's secure coding foundation.

## Input Validation

User input is represented by `UserInputModel`, which uses Data Annotation validation rules.

The username is:

- required;
- limited to between 2 and 50 characters;
- restricted through an allowlist of permitted characters.

The email address is:

- required;
- limited to 254 characters;
- validated as an email address.

The username allowlist rejects characters commonly associated with script markup and SQL-injection-style input.

The controller prevents invalid models from proceeding to database storage.

## SQL Injection Protection

The SafeVault data-access layer uses **parameterized SQLite commands** when inserting or retrieving user-controlled values.

Rather than concatenating user input into SQL command text, submitted values are supplied separately as SQL parameters.

This keeps user-controlled values separate from SQL command structure and provides protection against SQL injection.

---

# Activity 2: Authentication and Authorization

The second activity extended SafeVault with authentication and role-based access control.

## Secure Password Handling

SafeVault uses ASP.NET Core's password-hashing functionality through a dedicated `PasswordService`.

The service:

- hashes plaintext passwords before database storage;
- verifies submitted passwords against stored password hashes;
- accepts successful password verification results without comparing plaintext passwords directly.

Plaintext passwords are not stored in the SafeVault SQLite database.

Development account credentials are supplied through development configuration and are hashed before the accounts are written to the database.

## Authentication

SafeVault provides a login process that:

1. validates submitted credentials;
2. retrieves the matching authentication account;
3. verifies the submitted password against its stored hash;
4. creates identity claims for the authenticated account;
5. includes the account's role claim;
6. establishes the authenticated session.

Invalid credentials return the generic message:

> Invalid username or password.

The same message is used whether the username is unknown or the password is incorrect, avoiding unnecessary disclosure about which accounts exist.

## Role-Based Access Control

SafeVault implements two development roles:

- `user`
- `admin`

Administrative functionality is protected at the controller level with:

```csharp
[Authorize(Roles = "admin")]
```

Testing demonstrated three distinct access-control outcomes:

- an unauthenticated visitor attempting to access the Admin Dashboard is directed to authentication;
- an authenticated account with the `user` role is denied administrative access;
- an authenticated account with the `admin` role is permitted to access the Admin Dashboard.

This demonstrates the distinction between **authentication**, which establishes identity, and **authorization**, which determines what that identity is permitted to access.

## Additional Security Controls

SafeVault also uses:

- HTTPS redirection;
- antiforgery protection on POST forms;
- authentication and authorization middleware;
- a dedicated Access Denied response for authenticated accounts without sufficient permission;
- development-only account seeding.

---

# Activity 3: Security Debugging and Hardening

The third activity reviewed SafeVault specifically for SQL injection and cross-site scripting vulnerabilities.

## SQL Injection Review

The database code was audited for unsafe query construction.

No user-controlled values were found being concatenated directly into the reviewed SQL commands. The relevant insert, account lookup, and account-existence operations were already implemented using SQLite parameters.

Therefore, the secure implementation established during the earlier activities was retained rather than deliberately replacing working parameterized SQL.

## XSS Review

SafeVault was reviewed for unsafe handling of user-controlled content.

The username validation defined in `UserInputModel` restricts input through an allowlist of permitted characters, rejecting characters associated with HTML/script markup.

The reviewed Razor views use ordinary Razor expressions for dynamic output rather than deliberately rendering submitted values as raw HTML.

The application therefore combines restrictive validation of fields that do not require HTML with SafeVault's normal Razor output handling.

## Additional Hardening

During the Activity 3 review, the login username was given an explicit 50-character maximum length consistent with the username boundary used elsewhere in SafeVault.

This provides an additional input boundary while preserving the existing authentication design.

---

# Security Testing

Testing was performed throughout development rather than being postponed until the end of the project.

The test suite was expanded incrementally as new security features were introduced.

## Input Security Tests

Tests verify:

- legitimate input passes validation;
- empty input is rejected;
- invalid email input is rejected;
- SQL-injection-style username input is rejected;
- XSS-style username input is rejected.

## Password and Authentication Tests

Tests verify:

- password hashing does not return the original plaintext password;
- the correct password verifies successfully;
- an incorrect password fails verification;
- a missing login username is rejected;
- a missing login password is rejected.

## Authorization Tests

Tests verify:

- an authenticated admin identity is recognized as belonging to the `admin` role;
- an authenticated ordinary user is not incorrectly recognized as an administrator;
- `AdminController` explicitly requires the `admin` role.

## HTTP Integration Tests

SafeVault also uses ASP.NET Core integration tests to exercise security behavior through the application's request pipeline.

These tests verify:

- anonymous access to `/Admin` is redirected to authentication;
- an unknown username cannot authenticate;
- a known username with an incorrect password cannot authenticate;
- valid credentials establish an authenticated session;
- an authenticated ordinary user cannot access `/Admin`;
- an authenticated administrator can access `/Admin`.

## Attack-Scenario Tests

Activity 3 added HTTP-level attack tests specifically corresponding to the vulnerabilities identified by the Capstone instructions.

The tests submit:

- SQL-injection-style input through the SafeVault user-information form;
- XSS-style markup through the SafeVault user-information form.

The application rejects these submissions rather than treating them as valid user information.

### Final Automated Test Result

**21 tests executed**

**21 tests passed**

**0 tests failed**

The final successful test run was performed after the Activity 3 security review and hardening changes.

---

# Vulnerabilities Identified and Protections Applied

## SQL Injection

**Risk:** User-controlled input incorporated directly into SQL statements could alter the intended SQL operation.

**Finding:** The reviewed SafeVault database operations were already using parameterized SQL as a result of the secure implementation developed earlier in the Capstone.

**Protection:** Parameterized SQLite statements were retained and verified through unit-level and HTTP-level attack testing.

## Cross-Site Scripting

**Risk:** Untrusted content rendered unsafely into an HTML page could execute as browser script.

**Finding:** SafeVault already included restrictive validation for the original Username field, and the reviewed Razor output did not intentionally render submitted values as raw HTML.

**Protection:** Existing input validation and safe Razor output handling were retained and verified with XSS-style unit and integration tests.

## Unauthorized Access

**Risk:** An authenticated user could potentially access functionality outside the account's assigned permissions.

**Protection:** Role claims and `[Authorize(Roles = "admin")]` restrict administrative functionality to accounts assigned the `admin` role.

## Password Disclosure

**Risk:** Storing plaintext passwords would expose account credentials if application data were compromised.

**Protection:** Passwords are hashed before database storage and verified through `PasswordService`.

---

# How Microsoft Copilot Assisted

Microsoft Copilot was used throughout the SafeVault Capstone as a development and security-review assistant.

Copilot assisted with:

- generating and refining input-validation code;
- implementing parameterized database operations;
- reviewing the codebase for SQL-injection risks;
- reviewing form and Razor handling for potential XSS risks;
- implementing password hashing and password verification;
- implementing cookie-based authentication;
- establishing identity and role claims;
- implementing role-based access control;
- creating the protected Admin Dashboard;
- implementing Access Denied handling;
- generating unit and integration security tests;
- generating SQL-injection and XSS attack-scenario tests;
- diagnosing build and test failures;
- reviewing code comments and refactoring opportunities;
- identifying where proposed refactoring should be avoided because the existing implementation was already clear and secure.

Copilot suggestions were applied incrementally. After meaningful changes, the project was rebuilt and the automated test suite was rerun before further modifications were made.

---

# Development and Debugging Lessons

Several practices proved especially valuable during the project.

## Refactor Throughout Development

Refactoring was performed at controlled checkpoints instead of being postponed until the end of the project.

The project also demonstrated that **not every working implementation needs refactoring**. When a proposed formatting change to SQL introduced a regression, automated integration tests detected the problem and the known-good database code was restored.

## Maintain Known-Good Backups

A working backup was created before significant changes.

This provided safe recovery points during authentication, RBAC, integration testing, refactoring, and final security hardening.

## Test After Small Changes

Small development increments made failures easier to diagnose.

The test suite progressed from basic input validation to password security, authentication, authorization, integration testing, and attack-oriented testing.

## Do Not Weaken Secure Code Simply to Demonstrate a Fix

During Activity 3, the SQL audit found that SafeVault already used parameterized commands.

Rather than deliberately introducing SQL injection and then repairing it, the existing protection was preserved and verified through additional testing.

---

# Final Project Status

SafeVault now demonstrates:

- secure input validation;
- parameterized SQL database access;
- password hashing and verification;
- cookie-based authentication;
- role-based authorization;
- administrator-only functionality;
- antiforgery protection;
- controlled responses to unauthorized access;
- SQL-injection attack resistance;
- XSS-oriented input protection;
- unit-level security testing;
- HTTP integration security testing;
- attack-scenario testing.

The final known-good SafeVault codebase completed its automated security suite with:

> **21 tests passed, 0 tests failed.**

The project is ready for final documentation review and preparation for publication to a GitHub repository as the completed SafeVault Capstone submission.
