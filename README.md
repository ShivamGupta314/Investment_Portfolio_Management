# Investment Portfolio Management System

An ASP.NET Core MVC web application to manage investment portfolios with secure authentication, asset tracking, performance monitoring, risk profiling, and analytics dashboard. Built using SOLID principles and clean MVC architecture.

---

## 🚀 Features

- 🔐 User Authentication (JWT based)
- 🧾 Portfolio Management (Create, Clone, Delete, View)
- 💼 Asset Management per Portfolio
- 📊 Performance Visualization (Bar, Line, Pie Charts using Chart.js)
- 🛡️ Role-Based Authorization (User/Admin)
- 📉 Risk Assessment Module (Planned)
- 📁 Report Generation (Planned: PDF/CSV)
- 📈 Admin Analytics Dashboard
- ✅ Mobile Responsive UI (Bootstrap)
- ☑️ Two-Step Confirmation Before Delete

---

## 📁 Project Structure

```
InvestmentPortfolioManagement/
│
├── Controllers/               # MVC Controllers
├── Models/                    # Entity + ViewModels
├── Interfaces/                # Service Interfaces (SOLID)
├── Services/                  # Business Logic
├── Views/                     # Razor Views
│   └── Shared/                # Layout, Partials
├── wwwroot/                   # Static files (Bootstrap, JS, CSS)
├── appsettings.json           # Configuration + JWT settings
├── Program.cs                 # Middleware + DI config
├── README.md                  # Project overview
└── Documentation/             # Detailed technical + feature docs
```

---

## 🛠️ Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- JWT Authentication
- Bootstrap 5
- Chart.js (via CDN)
- SQL Server
- Visual Studio / Cursor

---

## 🔐 JWT Configuration (`appsettings.json`)

```json
"Jwt": {
  "Key": "your-secret-key",
  "Issuer": "your-app",
  "Audience": "your-users"
}
```

---

## ✅ How to Run

1. Clone the repo  
   `git clone https://github.com/yourusername/InvestmentPortfolioManagement.git`

2. Configure connection string in `appsettings.json`

3. Run migrations (if any)  
   `dotnet ef database update`

4. Build and run the app  
   `dotnet run`

---

## 📂 Documentation

See the `Documentation/` folder for:

- Feature Descriptions
- Architecture Diagrams
- SOLID Principle Implementation
- Sample Screenshots
- Future Improvements

---

## 🙋 Author

**Saurabh Suman**  
.NET Developer Intern – Cognizant  
B.Tech CSE, Cambridge Institute of Technology (2025)
