# Don't Repeat Yourself (DRY) principle

This guide enforces the **Don't Repeat Yourself (DRY)** principle in .NET projects.  
The goal is to reduce duplication in code, configuration, and logic to improve **maintainability, readability, and testability**.

---

## 1) Rules

### 1. Never duplicate knowledge in code.
    - If the same logic, configuration, or data structure appears in more than one place, consolidate it into a single, reusable abstraction.

---

## 2) Good Practices

### 1. Use Methods to Avoid Repeated Logic

// ❌ Bad: duplicate validation logic
```csharp
if (age < 18 || age > 99) { throw new ArgumentException("Invalid age"); }
...
if (age < 18 || age > 99) { throw new ArgumentException("Invalid age"); }
```

// ✅ Good: extract into a reusable method
```csharp
public void ValidateAge(int age)
{
    if (age < 18 || age > 99)
        throw new ArgumentException("Invalid age");
}


// Usage
ValidateAge(age);
```

---

### 2. Reuse Constants Instead of Magic Strings/Numbers

// ❌ Bad: repeated string literals
```csharp
if (role == "Admin") { ... }
...
if (role == "Admin") { ... }
```

// ✅ Good: centralize into constants
```csharp
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}


// Usage
if (role == Roles.Admin) { ... }
```

---

### 3. Apply Inheritance or Interfaces for Shared Behavior

// ❌ Bad: repeated method in multiple classes
```csharp
public class PdfReport {
    public void Export() { /* same code */ }
}

public class CsvReport {
    public void Export() { /* same code */ }
}
```
  
// ✅ Good: extract common behavior into a base class or interface
```csharp
public interface IReport
{
    void Export();
}

public abstract class ReportBase : IReport
{
    public void Export() { /* common export code */ }
}

public class PdfReport : ReportBase { }
public class CsvReport : ReportBase { }
```

---

### 4. Centralize Configuration

// ❌ Bad: repeated connection string
```csharp
var conn = new SqlConnection("Server=.;Database=AppDb;Trusted_Connection=True;");
...
var conn = new SqlConnection("Server=.;Database=AppDb;Trusted_Connection=True;");
```
  
// ✅ Good: move to configuration
```csharp
var conn = new SqlConnection(Configuration.GetConnectionString("AppDb"));
```

---

### 5. Use Generic Types or Helpers

// ❌ Bad: duplicate mapping code
```csharp
public UserDto Map(User user) { ... }

public ProductDto Map(Product product) { ... }
```

// ✅ Good: generic mapper
```csharp
public TTarget Map<TSource, TTarget>(TSource source)
{
    // generic mapping logic
}
```

---

## 3) Anti-Patterns to Avoid

- Copy-pasting code blocks across methods or classes.   
- Hardcoding values that may change in multiple locations. 
- Maintaining parallel methods that differ only slightly.
- Re-implementing existing framework features (e.g., LINQ, dependency injection).

---

## 4) Rule of Thumb

> **If you change one piece of knowledge, only one change should be required in the codebase.**