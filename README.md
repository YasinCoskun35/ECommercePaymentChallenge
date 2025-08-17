# E-Commerce Payment Integration Challenge

### Prerequisites
- .NET 9.0 SDK
- Docker & Docker Compose 

# Build and run
dotnet build
dotnet run --project src/ECommerce.Api

### Docker
```bash
# Build and run with Docker
docker-compose up --build

## 📋 API Endpoints

### Products
- `GET /api/products` - Retrieve products from external Balance Management service

### Orders
- `POST /api/orders/create` - Create new order with payment reservation
- `POST /api/orders/{id}/complete` - Complete order payment

## 🏗️ Project Structure

```
src/
├── ECommerce.Domain/          # Domain entities, value objects, exceptions
├── ECommerce.Application/     # Business logic, services, DTOs
├── ECommerce.Infrastructure/  # External services, repositories, data access
├── ECommerce.Api/            # Web API controllers, middleware
└── ECommerce.Tests/          # Unit and integration tests
```


### External Services
- **Balance Management API**: `https://balance-management-pi44.onrender.com`