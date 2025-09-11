# GameSpace - Gaming Community Platform

A comprehensive gaming community platform built with ASP.NET Core MVC, featuring forums, pet systems, mini-games, and e-commerce functionality.

## Quick Start

### Prerequisites
- Visual Studio 2022+ or Visual Studio Code
- SQL Server 2019/2022
- .NET 8.0 SDK

### Setup
1. Clone the repository
2. Run the database schema from `schema/database.sql` in SQL Server Management Studio
3. Update connection strings in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=GameSpaceIdentity;Trusted_Connection=true;TrustServerCertificate=true;",
       "GameSpace": "Server=localhost;Database=GameSpaceDatabase;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```
4. Build and run the application
5. Navigate to `https://localhost:5001`

### Health Checks
- General health: `GET /api/health`
- Database health: `GET /api/health/db`

## Project Structure

### Areas
- **MiniGame**: Pet system, mini-games, and user wallet functionality
- **Forum**: Discussion forums and community features
- **OnlineStore**: E-commerce and marketplace functionality
- **MemberManagement**: User account and profile management
- **social_hub**: Real-time chat and social features

### Key Features
- User authentication and authorization
- Pet care and mini-game system
- Daily check-in rewards
- Forum discussions and community features
- E-commerce with coupons and e-vouchers
- Real-time chat and notifications
- Admin management interface

## Development

### Technology Stack
- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Frontend**: Razor views with Bootstrap
- **Real-time**: SignalR for chat functionality
- **Authentication**: ASP.NET Core Identity

### Database
The database schema is defined in `schema/database.sql`. Do not use EF Migrations - the schema is the single source of truth.

### Fake Data
The system includes fake data generation to populate tables with exactly 200 rows each for development and testing purposes.

## Manual DB Initialization & Local Run

1. Ensure SQL Server is running
2. Execute `schema/database.sql` to create the database and initial data
3. Update connection strings in `appsettings.json`
4. Run the application
5. Access the health check endpoints to verify connectivity

### Troubleshooting
- **Connection Issues**: Verify SQL Server is running and connection strings are correct
- **Database Errors**: Ensure the schema has been properly executed
- **Build Errors**: Check that all NuGet packages are restored

## Admin Access
Default admin credentials:
- Username: `zhang_zhiming_01` / Password: `Password001@`
- Username: `li_xiaohua_02` / Password: `Password001@`

## Public Access
Sample user credentials:
- Username: `dragonknight88` / Password: `Password001@`