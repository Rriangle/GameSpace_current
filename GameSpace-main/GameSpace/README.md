# GameSpace Project

A comprehensive gaming platform with mini-games, user management, and social features.

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code

## Manual Database Setup

### 1. Create Database

```sql
-- Connect to your SQL Server instance
-- Create the database
CREATE DATABASE GameSpace;
GO

-- Use the database
USE GameSpace;
GO
```

### 2. Run Schema Script

```bash
# Navigate to the schema directory
cd schema

# Execute the database.sql script
sqlcmd -S (localdb)\MSSQLLocalDB -d GameSpace -i database.sql
```

### 3. Update Connection Strings

Update `appsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GameSpace_Identity;Trusted_Connection=true;MultipleActiveResultSets=true",
    "GameSpace": "Server=(localdb)\\mssqllocaldb;Database=GameSpace;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## Running the Application

1. **Restore Packages**
   ```bash
   dotnet restore
   ```

2. **Build the Application**
   ```bash
   dotnet build
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the Application**
   - Open browser to `https://localhost:5001`
   - MiniGame Area: `https://localhost:5001/MiniGame`

## Project Structure

```
GameSpace/
├── Areas/
│   ├── MiniGame/          # Mini-game functionality
│   ├── social_hub/        # Social features
│   └── ...
├── Controllers/           # Main controllers
├── Models/               # Data models
├── Data/                 # Database contexts
├── Views/                # Main views
└── schema/               # Database schema files
```

## Features

- **Mini Games**: Memory, Puzzle, Quiz games
- **User Management**: Registration, authentication, profiles
- **Pet System**: Virtual pet management
- **Wallet System**: Points and transaction history
- **Social Hub**: Chat and community features

## Development Notes

- Database schema is defined in `schema/database.sql`
- No EF migrations are used - manual schema management only
- All human-facing text is in English
- Areas are used for UI separation (Admin/Public)
- Serilog is configured for logging
- CorrelationId middleware is enabled for request tracking

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Verify connection strings in `appsettings.json`
- Check database permissions

### Build Errors
- Run `dotnet clean` then `dotnet build`
- Ensure all NuGet packages are restored
- Check .NET 8.0 SDK is installed

### Runtime Errors
- Check logs in `logs/` directory
- Verify database schema matches models
- Ensure all required services are registered