# 1️⃣ Imagen base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2️⃣ Imagen build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos TODO el proyecto
COPY . .

# Restauramos dependencias
RUN dotnet restore AuthService/AuthService.csproj

# Publicamos
RUN dotnet publish AuthService/AuthService.csproj -c Release -o /app/publish

# 3️⃣ Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]
