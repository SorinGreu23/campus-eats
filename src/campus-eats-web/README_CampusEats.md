
# CampusEats – Cafeteria Ordering System

CampusEats is a smart and modular cafeteria ordering system built using modern .NET technologies. It supports menu management, order processing, kitchen operations, payment integration, and a loyalty program.

---

## 🧩 Tech Stack

- **Backend:** .NET 8 Minimal API with Vertical Slice Architecture
- **Frontend:** Blazor WebAssembly *(optional)*
- **Database:** PostgreSQL / SQLite (for local dev)
- **Patterns:** CQRS, MediatR
- **Validation:** FluentValidation
- **Testing:** XUnit (unit tests), NSubstitute (integration tests)

---

## 📁 Project Structure

```
Features/
├── Menu/
├── Orders/
├── Kitchen/
├── Payments/
└── Loyalty/
```

Each feature follows Vertical Slice Architecture:
```csharp
// Example: CreateMenuItem
public record CreateMenuItemCommand(string Name, decimal Price) : IRequest<Result>;

public class CreateMenuItemHandler : IRequestHandler<CreateMenuItemCommand, Result> {
    public async Task<Result> Handle(CreateMenuItemCommand request, CancellationToken ct) {
        // Business logic here
    }
}

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemCommand> {
    public CreateMenuItemValidator() {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

app.MapPost("/menu", async (CreateMenuItemCommand cmd, IMediator mediator) => {
    var result = await mediator.Send(cmd);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});
```

---

## 🔑 Core Features

### 🍽️ Menu Management
- Add/update menu items with images, allergens
- Filter menu by category or dietary restrictions

### 🛒 Order Processing
- Place orders with multiple items
- View order history and status
- Cancel pending orders

### 👨‍🍳 Kitchen Operations
- View pending orders
- Update order status (Preparing → Ready → Completed)
- Daily inventory reports

### 💳 Payment Integration
- Mock or Stripe test payments
- Confirm payments via webhook
- View payment history

### 🎁 Loyalty Program
- View and redeem points
- Track loyalty transactions

---

## 👥 Team Responsibilities

- **Developer 1:** Menu feature + unit tests
- **Developer 2:** Orders + Kitchen features
- **Developer 3:** Payments + Loyalty + EF Core setup
- **Developer 4:** Blazor UI + integration tests + deployment

---

## 🧪 Testing Strategy

### XUnit Example:
```csharp
[Fact]
public async Task Should_Create_MenuItem_When_Valid() {
    var handler = new CreateMenuItemHandler(...);
    var command = new CreateMenuItemCommand("Soup", 5.99m);
    var result = await handler.Handle(command, CancellationToken.None);
    Assert.True(result.IsSuccess);
}
```

### NSubstitute Example:
```csharp
var repo = Substitute.For<IMenuRepository>();
repo.AddAsync(Arg.Any<MenuItem>()).Returns(Task.CompletedTask);
```

---

## 🗓️ Timeline (8 Weeks)

| Week | Tasks |
|------|-------|
| 1 | Setup repo, architecture, DB design |
| 2 | Menu feature + unit tests |
| 3 | Orders + Kitchen features |
| 4 | Payments + Loyalty features |
| 5 | Blazor UI: Menu & Orders |
| 6 | Blazor UI: Kitchen & Loyalty |
| 7 | Polish, test coverage |
| 8 | Documentation, deployment, presentation |

---
