dotnet ef migrations script -i --context ApplicationDbContext -o Scripts/Application.sql
dotnet ef migrations script -i --context ConfigurationDbContext -o Scripts/Configuration.sql
dotnet ef migrations script -i --context PersistedGrantDbContext -o Scripts/Operation.sql