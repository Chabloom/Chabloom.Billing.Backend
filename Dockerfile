FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
RUN ls
WORKDIR /Payments.Backend
RUN ls

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
RUN ls
WORKDIR /app
RUN ls
COPY --from=build-env /app/out .
RUN ls
ENTRYPOINT [ "Payments.Backend" ]
