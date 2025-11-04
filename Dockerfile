# ===== STAGE 1: Build .NET backend =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src

# Copia e restaura as dependências primeiro (para cache)
COPY C.sln .
COPY src/Consilium.API/Consilium.API.csproj src/Consilium.API/
COPY src/Consilium.Application/Consilium.Application.csproj src/Consilium.Application/
COPY src/Consilium.Domain/Consilium.Domain.csproj src/Consilium.Domain/
COPY src/Consilium.Infrastructure/Consilium.Infrastructure.csproj src/Consilium.Infrastructure/

# (Se tiveres mais .csproj, adiciona-os aqui)

RUN dotnet restore C.sln

# Copia o resto do código
COPY ./src ./src

# Publica a API
RUN dotnet publish "src/Consilium.API/Consilium.API.csproj" -c Release -o /app/publish

# ===== STAGE 2: Backend Production =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS build-backend-prod
WORKDIR /app
COPY --from=build-backend /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Consilium.API.dll"]