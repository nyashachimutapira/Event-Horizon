# Use the official .NET SDK to build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything and restore
COPY . .
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /app/publish

# Use ASP.NET Runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port (Render expects 10000+ but app uses 8080 or similar)
EXPOSE 8080

ENTRYPOINT ["dotnet", "EventManagementSystem.dll"]
