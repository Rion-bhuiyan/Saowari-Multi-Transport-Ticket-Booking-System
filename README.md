# 🚌 Saowari — Multi-Transport Ticket Booking System

> A centralized, intelligent, and fully automated online ticket booking and management platform for multi-modal transportation — covering **Bus**, **Launch (Vessel)**, and **Airplane** services.

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17+-DD0031?style=flat-square&logo=angular)](https://angular.io/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=flat-square&logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-Code_First-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-Academic-blue?style=flat-square)](#)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [System Architecture](#-system-architecture)
- [Database Design](#-database-design)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [User Roles](#-user-roles)
- [Refund Policy](#-refund-policy)
- [Team](#-team)

---

## 🌟 Overview

**Saowari** replaces manual, counter-based ticket sales with a robust digital infrastructure. The system enables customers to search, select, book, and pay for tickets from any device, at any time — while giving company management full real-time visibility into revenue, schedules, and operations.

### The Problem It Solves

| Current Problem | Saowari's Solution |
|---|---|
| Customers must physically visit ticket counters | 24/7 online booking from any device |
| Overbooking due to no centralized inventory | Real-time seat availability with atomic locking |
| Manual, error-prone revenue reporting | Automated dashboards with real-time financial data |
| No digital record of customer history | Persistent accounts with booking history |
| Counter agents can manipulate pricing | All transactions are system-controlled and audit-trailed |

---

## ✨ Features

### For Customers
- 🔍 **Unified Transport Search** — Search Bus, Launch, and Airplane from one interface
- 💺 **Interactive Seat Selection** — Visual seat map with color-coded availability
- 💳 **Multi-Gateway Payment** — bKash, Nagad, Rocket, Visa, Mastercard
- 🎫 **Instant E-Ticket + QR Code** — PDF e-ticket generated immediately after payment
- 🔔 **Trip Reminder Notifications** — Automated SMS & email reminders before departure
- ↩️ **Automated Refunds** — Policy-based refund calculation on cancellation
- 📋 **Booking History & Invoices** — Full travel history with downloadable PDF invoices

### For Admins
- 📊 **Real-Time Revenue Dashboard** — Live sales, route-wise income, payment statuses
- 🗺️ **Route & Schedule Management** — Add/edit/delete routes and departure timetables
- 🚌 **Fleet Registry** — Manage buses, vessels, and aircraft with maintenance tracking
- 💸 **Dynamic Pricing & Discounts** — Configure fares, promo codes, and seasonal discounts
- 📈 **Reports & Analytics** — Exportable Excel/PDF reports with occupancy and trend data
- 👥 **User & Role Management** — Full control over customer, agent, and admin accounts

### For Counter Staff
- 🖥️ **Assisted Booking Dashboard** — Simplified interface for walk-in customers
- 🖨️ **Physical Ticket Printing** — Print receipts for customers who prefer paper
- 📊 **Individual Sales Tracking** — Per-agent booking volumes and commission records

---

## 🛠 Tech Stack

| Layer | Technology | Purpose |
|---|---|---|
| **Frontend** | Angular 17+ & TypeScript | Single-Page Application (SPA) |
| **Styling** | Bootstrap / Angular Material / CSS3 | Responsive UI components |
| **Backend API** | ASP.NET Core Web API (.NET 8) | RESTful API & business logic |
| **ORM** | Entity Framework Core (Code First) | Database access & migrations |
| **Database** | Microsoft SQL Server | Relational data storage |
| **Authentication** | ASP.NET Core Identity + JWT | Secure auth & role-based access |
| **Version Control** | Git / GitHub | Source code management |
| **API Testing** | Postman / Swagger (Swashbuckle) | API documentation & testing |
| **PDF Generation** | iTextSharp | E-tickets & invoice generation |
| **Excel Reports** | EPPlus | Admin analytics export |
| **QR Codes** | QRCoder | Boarding verification |
| **Object Mapping** | AutoMapper | Entity ↔ DTO mapping |
| **Validation** | FluentValidation | API request validation |

---

## 🏗 System Architecture

Saowari follows a **Three-Tier Architecture**:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│    Angular 17+ (SPA) + TypeScript       │
│  Customer Portal  |  Admin Dashboard    │
└──────────────────┬──────────────────────┘
                   │ HTTP REST (JSON)
┌──────────────────▼──────────────────────┐
│      Application / API Layer            │
│   ASP.NET Core Web API (.NET 8)         │
│  Controllers → Services → Repositories  │
└──────────────────┬──────────────────────┘
                   │ EF Core (LINQ → SQL)
┌──────────────────▼──────────────────────┐
│           Data Layer                    │
│        Microsoft SQL Server             │
│  25 Tables | 2 Views | 2 Stored Procs   │
└─────────────────────────────────────────┘
```

---

## 🗄 Database Design

**SaowariDB** is a fully normalized SQL Server database (3NF) with **25 tables** across 9 domain groups, 2 views, and 2 stored procedures.

### Domain Groups

| Group | Tables | Description |
|---|---|---|
| **Auth & Users** | `Users`, `UserRoles`, `Roles`, `UserTokens` | Authentication, roles, JWT tokens |
| **Locations** | `Locations` | Terminals, ports, airports with GPS & search aliases |
| **Fleet** | `Vehicles`, `VehicleSeats` | Buses, vessels, aircraft and their seat layouts |
| **Operations** | `Routes`, `RouteStops`, `Schedules`, `ScheduleDepartures`, `DepartureSeatAvailability` | Routes, timetables, real-time seat matrix |
| **Pricing** | `FareClasses`, `Fares`, `DynamicPricingRules`, `PricingComponents` | Base fares, dynamic pricing, VAT/service charges |
| **Booking** | `Bookings`, `BookingPassengers`, `BookingSeats` | Master booking records, passenger & seat assignment |
| **Payments** | `PaymentMethods`, `Payments` | All payment transactions and gateway records |
| **E-Tickets** | `Tickets` | E-ticket issuance, QR codes, boarding audit trail |
| **Cancellation** | `CancellationPolicies`, `CancellationRequests`, `RefundTransactions` | Tiered refund rules and refund processing |

### Key Constraints & Rules

- `UNIQUE(DepartureID, SeatID)` in `BookingSeats` — **database-level double-booking prevention**
- `sp_LockSeat` stored procedure — **atomic seat locking** inside `BEGIN TRANSACTION / TRY/CATCH`
- `sp_ReleaseExpiredLocks` — auto-releases held seats every 60 seconds
- `v_AvailableTrips` view — pre-joined trip search results for high-performance search API
- `v_BookingSummary` view — flat booking record eliminating 8-table joins in the admin panel

### EF Core DbContext

The `SaowariDbContext` registers all 25 entities and configures relationships with `DeleteBehavior.Restrict` to prevent accidental cascade deletes across the booking chain.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) & [Angular CLI](https://angular.io/cli)
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server) or SQL Server Express
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or VS Code

### Backend Setup

```bash
# Clone the repository
git clone https://github.com/SKahasun/Saowari-Multi-Transport-Ticket-Booking-System
cd Saowari-Multi-Transport-Ticket-Booking-System

# Restore dependencies
dotnet restore

# Update connection string in appsettings.json
# "ConnectionStrings": { "DefaultConnection": "Server=...;Database=SaowariDB;..." }

# Apply EF Core migrations
dotnet ef database update

# Run the API
dotnet run --project Saowari.API
```

The API will be available at `https://localhost:7001` with Swagger UI at `/swagger`.

### Frontend Setup

```bash
cd saowari-client

# Install dependencies
npm install

# Start the development server
ng serve -o
```

The Angular app will be available at `http://localhost:4200`.

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project Saowari.Data --startup-project Saowari.API

# Apply migrations
dotnet ef database update --project Saowari.Data --startup-project Saowari.API
```

---

## 📁 Project Structure

```
Saowari/
├── Saowari.API/              # ASP.NET Core Web API
│   ├── Controllers/          # API endpoint controllers
│   ├── Program.cs            # App configuration & middleware
│   └── appsettings.json      # Connection strings & settings
│
├── Saowari.Core/             # Business logic & interfaces
│   ├── Services/             # Service implementations
│   ├── DTOs/                 # Data Transfer Objects
│   └── Interfaces/           # Repository & service interfaces
│
├── Saowari.Data/             # Data access layer
│   ├── SaowariDbContext.cs   # EF Core DbContext
│   ├── Migrations/           # EF Core migration files
│   └── Repositories/         # Repository implementations
│
├── Saowari.Models/           # Domain entities
│   └── Entities/             # All 25 entity classes
│
└── saowari-client/           # Angular 17+ frontend
    ├── src/app/
    │   ├── features/         # Feature modules (booking, admin, etc.)
    │   ├── shared/           # Shared components & services
    │   └── core/             # Guards, interceptors, auth
    └── angular.json
```

---

## 👥 User Roles

| Role | Access |
|---|---|
| **Customer** | Search trips, book tickets, manage profile, view booking history, request cancellations |
| **Counter Staff / Agent** | All customer actions on behalf of walk-in customers, print physical tickets, process counter-side cancellations |
| **Admin** | Full system control — routes, schedules, pricing, fleet, all bookings, revenue reports, user management, refund approvals |

---

## 💰 Refund Policy

Refund amounts are calculated **automatically** by the system based on time before departure:

| Transport | 72+ Hours | 24–72 Hours | < 24 Hours |
|---|---|---|---|
| **Bus** | 100% | 50% | 0% |
| **Launch / Vessel** | 100% | 50% | 0% |
| **Airplane** | 80% | 50% | 20%* |

> *Airplane < 24 hour refund applies only for documented medical emergencies. All refunds are returned to the original payment method.

---

## 👨‍💻 Team

This project was developed as part of the **IsDB-BISEW IT Scholarship Programme** (Batch: WADA/PANTL-M/69/01) for the course *Web Application Development Using ASP.NET* at **PeopleNTech Institute**.

| Student Name | Student ID | Role |
|---|---|---|
| Sheikh Ahasunul Islam | 1294372 | 👑 Team Leader |
| Nakibul Islam Fahim | 1294702 | 🪖 Co Leader |
| Shamsul Arefin | 1294499 | Team Member |
| Ahsan-Ul-Hafeez | 1294650 | Team Member |
| Al Arafat Bhuiyan | 1294365 | Team Member |
| Md. Saiyadul Islam | 1294803 | Team Member |

**Trainer:** Md. Azman Ali, Sr. Faculty (ASP.NET), PeopleNTech Institute  
**Submitted To:** Syed Zahidul Hassan, Consultant, Show & Tell Consulting Ltd  
**Submission Date:** 12 April 2026

---

*Saowari — Making transport accessible for every Bangladeshi, from anywhere, at any time.*
