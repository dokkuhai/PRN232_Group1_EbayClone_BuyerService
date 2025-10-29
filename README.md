#  PRN232_Group1_EbayClone_BuyerService

**EbayClone BuyerService** is a microservice within the **EbayClone System** – an e-commerce platform inspired by eBay, built with **.NET 8 Web API** using **Clean Architecture** and containerized for deployment.

The BuyerService manages all operations for **Buyer** users, including:
- Browsing and searching products  
- Managing shopping carts and orders  
- Handling buyer profiles  
- Communicating with other microservices in the EbayClone ecosystem

The project is fully Dockerized, using **Nginx Reverse Proxy** for load balancing multiple API instances, and **GitHub Actions** for automated CI/CD deployment to **Amazon EC2**.

---

##  System Architecture

```
┌───────────────────────────────┐
│          NGINX (8085:80)      |
│ Reverse Proxy + Load Balancer │
└──────────────┬────────────────┘
               │
               ▼
            (8081)               (8081)
   ┌───────────────────┐     ┌───────────────────┐
   │  API Instance #1  │     │  API Instance #2  │
   │ ASP.NET Core 8.0  │     │ ASP.NET Core 8.0  │
   └───────────────────┘     └───────────────────┘
               │
               ▼
           (3307:3306)
        ┌──────────────┐
        │   MySQL DB   │
        │   (8.0)      │
        └──────────────┘
```

---

##  Project Structure

```
PRN232_Group1_EbayClone_BuyerService/
│
├── Dockerfile                              # Build file for API
├── docker-compose.yml                      # Orchestration stack
│
├── EbayClone.BuyerService.CoreAPI/         # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Models/
│   |   ├── Requests/
│   |   ├── Reponses/   
│   ├── Repository/
|   │   ├── Impl/
|   │   ├── Interface/
│   ├── Service/
│   |   ├── Impl/
│   |   ├── Interface/   
│   ├── Utils/
│   ├── Properties
│   └── Program.cs
│
├── db/
│   └── clone_ebay_mysql_schema.sql         # Initial MySQL schema
│
├── nginx/
│   └── nginx.conf                          # Load balancing + proxy config
│
├── ui/                                     # Static web UI 
│   └── index.html
│
└── .github/workflows/ci-cd.yml             # CI/CD pipeline (GitHub Actions)
```

---

## Core Components


### 2. **Models**
- Located in `EbayCloneBuyerService_CoreAPI/Models/`
- Contains **data models**, **DTOs**, and **request/response objects** for the API.
- Divided into:
  - `Requests/` → input DTOs for client requests  
  - `Responses/` → output DTOs for API responses  
- These models define the structure of data exchanged between frontend and backend.

> Example:  
> - Request: `CreateOrderRequest`, `LoginRequest`  
> - Response: `ProductResponse`, `OrderDetailResponse`

### 3. **Repositories**
- Located in `EbayCloneBuyerService_CoreAPI/Repositories/`
- Handles all **data access logic** with two main parts:
  - `Interface/` → repository interfaces (e.g., `IProductRepository`, `IOrderRepository`)
  - `Impl/` → implementation classes for data access (e.g., `ProductRepository`, `OrderRepository`)
- Communicates directly with the database via EF Core or raw SQL queries.

> Example:  
> `ProductRepository` implementing `IProductRepository`

### 4. **Services**
- Located in `EbayCloneBuyerService_CoreAPI/Services/`
- Encapsulates **business logic**.
- Divided into:
  - `Interface/` → service contracts (e.g., `IOrderService`, `ICartService`)
  - `Impl/` → service implementations (e.g., `OrderService`, `CartService`)
- Services use repositories to retrieve and modify data, applying business rules and validations.

> Example:  
> `BuyerService` implementing `IBuyerService`  
> `AuthService` implementing `IAuthService`

### 5. **Utils**
- Located in `EbayCloneBuyerService_CoreAPI/Utils/`
- Contains **helper classes**, **extensions**, and **utility functions** commonly used across controllers, repositories, and services.

> Example: `JwtHelper`, `PasswordHasher`, `DateTimeExtensions`

---

##  Dockerized Architecture

###  Run locally

```bash
docker-compose up --build -d
```

After starting:

| Service | Port | Description |
|----------|------|-------------|
| Nginx (Reverse Proxy) | `8085` | Public entrypoint |
| API #1 | Internal (8081) | API instance 1 |
| API #2 | Internal (8081) | API instance 2 |
| MySQL | `3307` | Database service |

Access:
- UI → http://localhost:8085  
- API → http://localhost:8085/api  
- Swagger → http://localhost:8085/swagger/index.html  

---

### Load Balancing

`nginx.conf` defines an upstream pool for both API instances:

```nginx
upstream api_upstream {
    server api1:8081;
    server api2:8081;
}
```

All requests to `/api/` and `/swagger` are proxied through Nginx to `api_upstream` for **round-robin load balancing**.

---

## CI/CD Pipeline (GitHub Actions)

Automated build & deployment pipeline to Amazon EC2:

1. **Build Phase**
   - Build API from root `Dockerfile`
   - Push image to Docker Hub (`ebayclone-buyer-api:latest`)

2. **Deploy Phase**
   - SSH into EC2 (`ec2-user`)
   - Pull latest repo & Docker image
   - Run `docker-compose up -d --build`
   - Nginx automatically reloads

Workflow: `.github/workflows/ci-cd.yml`

> Required GitHub Secrets:
> - `DOCKER_USERNAME`, `DOCKER_PASSWORD`  
> - `SERVER_IP`, `SERVER_SSH_KEY`

---

## Database Initialization

- On the first MySQL container startup, `clone_ebay_mysql_schema.sql` is automatically imported.
- Data is stored persistently in the named Docker volume `db_data`.
- Rebuilds or restarts **will not remove data** unless the volume is deleted manually.

>  Data is lost only when removing the volume using `docker-compose down -v` or `docker volume rm db_data`.

---

##  Useful Docker Commands

| Command | Description |
|----------|-------------|
| `docker-compose up -d --build` | Build & start all services |
| `docker-compose down` | Stop all containers (preserve data) |
| `docker-compose down -v` | Stop & delete database volume (erase data) |
| `docker logs <container>` | View container logs |
| `docker exec -it ebayclone_buyer_db bash` | Access MySQL container shell |

---

##  Database Backup & Restore

Backup:
```bash
docker exec ebayclone_buyer_db mysqldump -uroot -proot CloneEbayDB > backup.sql
```

Restore:
```bash
docker exec -i ebayclone_buyer_db mysql -uroot -proot CloneEbayDB < backup.sql
```
