# PRN232_Group1_EbayClone_BuyerService
EbayClone is an online marketplace system inspired by eBay, built with .NET 8 Web API following Clean Architecture. It includes modules such as WebApi, Service, Service.Contracts, Repository, Contracts, Shared, and LoggerService, supporting RESTful APIs, JWT authentication, Swagger/OpenAPI, logging, exception handling, and validation.

This project is part of the **EbayClone Microservices System**, focusing on the **Buyer Service**.  
The Buyer Service handles user interactions related to browsing, searching, and purchasing products.

This README provides an **overview of the solution architecture** and explains the purpose of each core component.  
Each component will later have its own detailed documentation.

---

## 📂 Project Structure

## 🧩 Components Overview

### 1. Contracts
- Contains **interfaces** that define the core contracts for repositories and services.
- Ensures **loose coupling** between layers.
- Allows easy unit testing and dependency injection.

> Example: `IProductRepository`, `IBuyerService`.

---

### 2. Entities
- Represents the **domain models** and **database entities**.
- Mapped directly to database tables via **ORM (EF Core / Dapper)**.
- May include entity configurations, relationships, and constraints.

> Example: `Product`, `Order`, `BuyerProfile`.

---

### 3. LoggerService
- Provides a **centralized logging mechanism**.
- Wraps around logging libraries (e.g., Serilog, NLog, Microsoft.Extensions.Logging).
- Ensures consistent logging across services and repositories.

> Example: `ILoggerManager` with methods like `LogInfo`, `LogWarn`, `LogError`.

---

### 4. Repository
- Implements **data access logic**.
- Interacts with the database through Entities.
- Only focuses on CRUD and query operations.

> Example: `ProductRepository` implementing `IProductRepository`.

---

### 5. Service
- Contains **business logic**.
- Calls repositories to fetch/update data.
- Applies rules, validations, and transformations.
- Works as the main layer between **Controllers** and **Repositories**.

> Example: `BuyerService` implementing `IBuyerService`.

---

### 6. Service.Contracts
- Defines **interfaces for services**.
- Promotes abstraction and ensures services can be tested or replaced independently.

> Example: `IBuyerService`, `IOrderService`.

---

### 7. Shared
- Contains **common utilities, DTOs, constants, and exception classes** shared across layers.
- Prevents duplication of code and promotes consistency.

> Example:
  - **DTOs**: `ProductDto`, `OrderDto`.
  - **Exceptions**: `NotFoundException`, `ValidationException`.
  - **Constants**: `AppConstants`.

---

## 🔗 Development Guidelines

1. **Follow Dependency Inversion Principle**  
   - Always depend on **Contracts**, not implementations.

2. **Keep Layers Independent**  
   - Services should not directly depend on Entities. Use DTOs from `Shared`.

3. **Use Dependency Injection**  
   - Register all services and repositories in `Program.cs` / `Startup.cs`.

4. **Centralized Logging & Error Handling**  
   - Always use `LoggerService` for logging.  
   - Use `Shared.Exceptions` for standardized error handling.

