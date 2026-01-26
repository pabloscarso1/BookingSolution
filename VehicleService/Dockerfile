# 1️⃣ Imagen base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2️⃣ Imagen build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos TODO el proyecto
COPY . .

# Restauramos dependencias
RUN dotnet restore VehicleService/VehicleService.csproj

# Publicamos
RUN dotnet publish VehicleService/VehicleService.csproj -c Release -o /app/publish

# 3️⃣ Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VehicleService.dll"]
