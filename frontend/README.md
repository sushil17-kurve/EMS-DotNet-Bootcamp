# 🏢 Employee Management System (EMS)

A full-stack, production-grade Employee Management System built with 
**ASP.NET Core 9 Web API** and **React 18**. Demonstrates enterprise-level 
architecture patterns and real-world development practices.

---

## 🚀 Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| ASP.NET Core 9 Web API | REST API framework |
| Entity Framework Core 9 | ORM — Code First approach |
| SQL Server | Relational database |
| JWT + Refresh Tokens | Authentication & Authorization |
| Serilog | Structured logging |
| FluentValidation | Input validation |
| AutoMapper | Object-to-object mapping |
| BCrypt.Net | Password hashing |

### Frontend
| Technology | Purpose |
|---|---|
| React 18 + Vite | Frontend framework |
| Material UI (MUI) v6 | UI component library |
| Axios | HTTP client with interceptors |
| React Hook Form + Yup | Form handling & validation |
| Recharts | Dashboard charts |
| React Router v6 | Client-side routing |

---

## 🏗️ Architecture

### Design Patterns Used
- ✅ Clean Architecture (4-layer separation)
- ✅ Repository Pattern
- ✅ Unit of Work Pattern
- ✅ Dependency Injection
- ✅ DTO Pattern
- ✅ Middleware Pipeline

---

## ✨ Features

### Authentication & Security
- JWT Access Token (15 min) + Refresh Token (7 days)
- BCrypt password hashing
- Role-based authorization (SuperAdmin / Admin / Employee)
- Token rotation on refresh
- Server-side logout (invalidates refresh token)

### Employee Management
- Full CRUD with soft delete
- Profile photo upload
- Auto-generated employee codes (EMP-0001)
- Search, filter by department/status, pagination

### Department Management
- CRUD with business rules
- Cannot delete department with active employees
- Employee headcount per department

### Leave Management
- Apply, approve, reject, cancel leave requests
- Leave balance tracking per type per year
- Overlap detection (no double bookings)
- Admin review workflow

### Dashboard
- Real-time stats (total employees, pending leaves)
- Bar chart — monthly joinings (last 6 months)
- Pie chart — leave status distribution
- Horizontal bar — department headcount
- Recent activity feed

---

## 🚦 Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (Developer Edition)
- Node.js 18+

### Backend Setup
```bash
# Clone the repo
git clone https://github.com/YOUR_USERNAME/EMS-DotNet-Bootcamp.git
cd EMS-DotNet-Bootcamp

# Update connection string in src/EMS.API/appsettings.json

# Run API (auto-migrates and seeds DB)
dotnet run --project src/EMS.API
```

### Frontend Setup
```bash
cd frontend

# Create .env file
echo "VITE_API_BASE_URL=https://localhost:7226/api" > .env

npm install
npm run dev
```

### Default Login
| Role | Email | Password |
|---|---|---|
| Super Admin | superadmin@ems.com | Admin@123 |

---

## 📸 Screenshots

> Add screenshots here after taking them

---

## 🔑 API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/login | ❌ | Login |
| POST | /api/auth/register | ❌ | Register |
| POST | /api/auth/refresh-token | ❌ | Refresh JWT |
| POST | /api/auth/logout | ✅ | Logout |
| GET | /api/employees | ✅ | List employees (paginated) |
| POST | /api/employees | Admin | Create employee |
| PUT | /api/employees/{id} | Admin | Update employee |
| DELETE | /api/employees/{id} | Admin | Deactivate employee |
| GET | /api/departments | ✅ | List departments |
| POST | /api/departments | Admin | Create department |
| GET | /api/leaverequests | Admin | All leave requests |
| POST | /api/leaverequests | ✅ | Apply for leave |
| PATCH | /api/leaverequests/{id}/review | Admin | Approve/Reject |
| GET | /api/dashboard/stats | Admin | Dashboard statistics |

---

## 🧠 Key Learning Outcomes

1. **Clean Architecture** — How to structure a .NET solution for maintainability
2. **JWT Security** — Access + refresh token flow with rotation
3. **Repository + UoW** — Data access abstraction and testability
4. **EF Core Code First** — Fluent API configuration, migrations, seeding
5. **React + API** — Axios interceptors, protected routes, global state
6. **Production Practices** — Logging, error handling, validation layers

---

## 👤 Author
 
[GitHub](https://github.com/YOUR_USERNAME) · 
