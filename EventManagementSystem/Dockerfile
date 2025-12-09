# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["EventManagementSystem.csproj", "."]
RUN dotnet restore "EventManagementSystem.csproj"
COPY . .
RUN dotnet build "EventManagementSystem.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EventManagementSystem.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventManagementSystem.dll"]
