FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

RUN ls
COPY global.json ./
COPY Payments.Backend/ ./
RUN ls
RUN dotnet clean
RUN ls
RUN dotnet restore
RUN ls
RUN dotnet publish -c Release -o out
RUN ls

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "Payments.Backend" ]
