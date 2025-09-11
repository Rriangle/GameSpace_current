# GameSpace - Game Forum Platform

A comprehensive game forum platform built with ASP.NET Core MVC, featuring community discussions, mini-games, online store, and social features.

## Features

- **Forum System**: Game discussions and community threads
- **Mini Game System**: Pet care mini-games and daily check-ins
- **Online Store**: Game products and merchandise
- **Member Management**: User profiles and account management
- **Social Features**: Real-time chat and notifications

## Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Razor Pages, Bootstrap, jQuery, Vue.js
- **Real-time**: SignalR for chat and notifications
- **Authentication**: ASP.NET Core Identity

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
2. Update connection strings in `appsettings.json`
3. Run database migrations
4. Build and run the application

### Configuration

Update the following connection strings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your Identity DB Connection String",
    "GameSpace": "Your Main DB Connection String"
  }
}
```

## Project Structure

- `Areas/` - Modular areas for different features
- `Models/` - Entity models and database context
- `Controllers/` - MVC controllers
- `Views/` - Razor view templates
- `wwwroot/` - Static assets (CSS, JS, images)

## Health Checks

The application provides health check endpoints:

- `/healthz` - General application health
- `/healthz/db` - Database connectivity status

## Development

This project follows a strict development workflow with:

- English-only human-facing text
- Database schema as single source of truth
- Area-based modular architecture
- Comprehensive logging with Serilog

## License

This project is proprietary software.