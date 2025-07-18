# Solution Overview

The main purpose of this solution is to serve as an **AutoCAD plugin with a modern WPF-based user interface**.  
It is a multi-project .NET ecosystem targeting `.NET Framework 4.7.2`, `.NET Core`, and `.NET Standard`.  
The solution includes desktop, web, and library components, integrating a variety of modern technologies.

## Main Technologies Used

### 1. **WPF (Windows Presentation Foundation)**
- **Purpose:** Used for building rich desktop user interfaces.
- **Usage:** Several projects (e.g., `TaskDialog`, `ID.Data`) use WPF for custom dialogs, controls, and editors.
- **Libraries:** Includes XAML-based controls, custom editors, and advanced UI features.

### 2. **ASP.NET Core & ASP.NET Web API**
- **Purpose:** For building RESTful web APIs and web applications.
- **Usage:** Projects like `Mapit.WebApi` and `MapIt.WebApp` provide HTTP services and web interfaces.
- **Frameworks:** Uses both ASP.NET Core (for modern, cross-platform APIs) and classic ASP.NET Web API.

### 3. **SignalR**
- **Purpose:** Real-time web functionality (e.g., live updates, notifications).
- **Usage:** Integrated in projects such as `ID.SignalRSelfHost` and `ID.AcadNet` for real-time communication between server and clients.

### 4. **Entity Framework Core**
- **Purpose:** Object-relational mapping (ORM) for database access.
- **Usage:** Used for data access in .NET Core projects, supporting SQL Server and PostgreSQL via `Npgsql`.

### 5. **PostgreSQL**
- **Purpose:** Open-source relational database.
- **Usage:** Data storage for several backend projects, accessed via `Npgsql` and `EntityFrameworkCore`.

### 6. **Xceed WPF Toolkit**
- **Purpose:** Advanced WPF controls (e.g., property grids, editors).
- **Usage:** Enhances WPF UI with additional controls in projects like `ID.Data`.

### 7. **Owin/Katana**
- **Purpose:** Middleware pipeline for .NET web applications.
- **Usage:** Used for self-hosting web APIs and SignalR endpoints.

### 8. **Serilog**
- **Purpose:** Structured logging for .NET applications.
- **Usage:** Provides logging infrastructure across web and service projects.

### 9. **AutoMapper**
- **Purpose:** Object-to-object mapping.
- **Usage:** Simplifies data transfer between layers (DTOs, entities, view models).

### 10. **Newtonsoft.Json**
- **Purpose:** JSON serialization/deserialization.
- **Usage:** Used for data exchange in APIs and configuration.

### 11. **Swagger (Swashbuckle)**
- **Purpose:** API documentation and testing UI.
- **Usage:** Automatically generates OpenAPI documentation for web APIs.

### 12. **ASP.NET Identity**
- **Purpose:** Authentication and user management.
- **Usage:** Includes support for PostgreSQL-backed identity stores.

## Target Frameworks

- **.NET Framework 4.7.2:** For legacy and desktop applications.
- **.NET Core:** For modern, cross-platform web and service projects.
- **.NET Standard:** For shared libraries compatible across .NET implementations.

## Solution Structure

- **Desktop:** WPF-based UI, dialogs, and editors.
- **Web:** REST APIs, web applications, and real-time communication.
- **Libraries:** Shared utilities, models, and infrastructure.

---

**Note:** This solution leverages a modular architecture, allowing for code reuse and integration across desktop and web environments.
