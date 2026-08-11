# Financial Monitor - Real-Time Transaction Processing & Analytics

A robust, high-performance financial monitoring system designed to handle real-time transaction processing, duplicate request prevention, and dynamic data streaming.

---

##  About The Project

**Financial Monitor** is an enterprise-grade backend and frontend system built to capture, validate, and broadcast financial transactions in real time. The project demonstrates a modern event-driven architecture that combines relational database persistence, high-speed distributed caching, idempotency protection, and real-time WebSocket communication.

### Key Capabilities
* **Idempotency Protection:** Prevents duplicate transaction processing using Redis-based distributed locks (`X-Idempotency-Key`).
* **Caching Strategy:** Implements the **Decorator Pattern** over repository layers to minimize database load using Redis read/write caching.
* **Real-Time Streaming:** Uses **SignalR** with a Redis backplane to push transaction updates instantly to connected clients.
* **Scalable Architecture:** Clean separation of concerns using Dependency Injection, Repository Pattern, and DTOs.

---

##  Tech Stack

### **Backend (.NET Core / C#)**
* **Framework:** ASP.NET Core Web API
* **Database & ORM:** PostgreSQL + Entity Framework Core
* **Caching & Locking:** StackExchange.Redis
* **Real-Time Communications:** SignalR (with Redis Scale-out Backplane)
* **Testing:** xUnit, Moq, FluentAssertions
* **Logging:** Microsoft.Extensions.Logging

### **Frontend (React / TypeScript)**
* **Core:** React, TypeScript, Vite / Create React App
* **Real-Time Client:** `@microsoft/signalr`
* **Utilities:** `uuid` (for Idempotency key generation)

---

##  Getting Started on a New Machine

Follow these instructions to clone, configure, and run the project locally on a clean setup.

### **Prerequisites**
Make sure you have the following installed on your machine:
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or later)
* [Node.js](https://nodejs.org/) (v18+ recommended) + `npm`
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) *(Recommended for quick infrastructure setup)* **OR** local instances of:
  * **PostgreSQL** (Port `5432`)
  * **Redis** (Port `6379`)

---

### **1. Infrastructure Setup (Docker)**

The easiest way to run PostgreSQL and Redis locally is via Docker:

```bash
# Run PostgreSQL container
docker run --name financial-postgres -e POSTGRES_DB=FinancialDb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres

# Run Redis container
docker run --name financial-redis -p 6379:6379 -d redis
