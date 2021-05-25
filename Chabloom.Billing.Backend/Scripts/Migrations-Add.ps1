dotnet ef migrations add ApplicationMigration1 --context ApplicationDbContext -o Data/Migrations/Application
dotnet ef migrations add ConfigurationMigration1 --context ConfigurationDbContext -o Data/Migrations/Configuration
dotnet ef migrations add OperationMigration1 --context PersistedGrantDbContext -o Data/Migrations/Operation