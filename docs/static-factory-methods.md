# Static Factory Method Pattern

This guide shows how to implement static factory methods for complex object creation with dependency injection.

## Recommended Pattern - Static Factory Method on Class

```csharp
using GameKit;

public class MyService
{
    public static MyService Create(IDependency1 dependency1, IDependency2 dependency2, IDependency3 dependency3)
    {
        // Create complex objects using dependencies
        var complexObject = dependency1.CreateComplexObject(dependency2.CreateDifferentObject());

        // 3. Instantiate and return the service
        return new MyService(complexObject, dependency3);
    }

    // private, so it's not exposed
    private MyService(ComplexObject complexObject, IDependency3 dependency3)
    {
        // Constructor logic
    }
}
```

## When to Use

Use static factory methods when:
- Construction involves complex setup or configuration
- You need to create intermediate objects before final instantiation
- Constructor would become cluttered with setup logic and may raise an exception
