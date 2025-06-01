# Investment Portfolio Management System

An ASP.NET Core MVC-based web application for managing investment portfolios with performance tracking, asset management, risk assessment, report generation, and analytics. This project follows SOLID principles and uses JWT authentication.

---

## 🔧 Technologies Used
- .NET 8
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- JWT Authentication
- Bootstrap 5
- Chart.js
- DinkToPdf (PDF export)
- CSV Export (via StringBuilder)

---

## ✅ Features
- **User Authentication** (Register/Login using JWT)
- **Portfolio Management** (Add/Edit/Delete/View)
- **Asset Tracking** (Linked to Portfolios)
- **Risk Module** (Risk score per portfolio)
- **Performance Module** (Chart.js based visualization)
- **Report Module** (PDF & CSV Export)
- **Analytics Dashboard** (For Admin to view all users)
- **Role-Based Authorization** (User/Admin)
- **Responsive UI** with Bootstrap
- **ViewModels & Interfaces** following SOLID principles

---

## 📁 Folder Structure
```
InvestmentPortfolioManagement/
├── Controllers/
├── Models/
├── Views/
├── Services/
├── Interfaces/
├── Data/
├── Helpers/
├── Documentation/
├── Native/ (DLL file for PDF)
├── wwwroot/
├── appsettings.json
├── Program.cs
├── InvestmentPortfolioManagement.sln ✅
```

---

## 🚀 How to Run (Windows - Visual Studio Community)

### 1. Clone the Repository
```bash
git clone <your-repo-url>
cd InvestmentPortfolioManagement
```

### 2. Open in Visual Studio
- Open `InvestmentPortfolioManagement.sln`

### 3. Install Required NuGet Packages
Make sure these are installed:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```
```bash
dotnet add package DinkToPdf
```
```bash
dotnet add package DinkToPdf.Contracts
```
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### 4. Setup Database
Update your `appsettings.json` connection string:
```json
"ConnectionStrings": {
  "ConnectionString": "Server=YOUR_SQL_SERVER;Database=PortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
Run migrations:
```bash
dotnet ef migrations add InitialCreate
```
```bash
dotnet ef database update
```

---

## 🖨 PDF Export Setup

### Step 1: Download Native DLL
Download `libwkhtmltox.dll` from:
https://github.com/rdvojmoc/DinkToPdf/tree/master/v0.12.4/64%20bit

### Step 2: Place in Folder
Put `libwkhtmltox.dll` into:
```
InvestmentPortfolioManagement/Native/
```

### Step 3: Code to Load DLL (Program.cs)
```csharp
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "Native", "libwkhtmltox.dll"));
```
Also add:
```csharp
using InvestmentPortfolioManagement.Helpers;
```

### Step 4: Register PDF Service
```csharp
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
```

---

## 🔐 Default Roles
- **Admin** can access analytics
- **User** can manage their portfolios

---

## 📁 Documentation
Check `Documentation/` folder for:
- Module breakdown
- Diagrams & charts (performance, risk)
- Screenshots (if needed)

---

## 📦 Submission Ready
- ✅ Compatible with Windows Visual Studio
- ✅ Self-contained `.sln` for one-click run
- ✅ Mobile responsive UI
- ✅ Clean, modular, and documented codebase

---

## 🙌 Author
This project was developed as part of an internship assignment by [Shivam Gupta] under guidance from mentors using best coding practices.
