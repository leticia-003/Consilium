# ===== STAGE 1: Build Angular =====
# (No changes needed for this stage)
FROM node:20 AS build-frontend
WORKDIR /app
COPY src/frontend/package*.json ./ 
RUN npm install
COPY src/frontend/ .
RUN npm run build

# ===== STAGE 2: Frontend Production (nginx) =====
# (No changes needed for this stage)
FROM nginx:alpine AS build-frontend-prod
COPY --from=build-frontend /app/dist/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

# ===== STAGE 3: Build .NET backend (Updated) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src

# Copy the solution file and all project files first
# This caches the dependency layer
COPY C.sln .
COPY src/Consilium.API/Consilium.API.csproj src/Consilium.API/
COPY src/Consilium.Application/Consilium.Application.csproj src/Consilium.Application/
COPY src/Consilium.Domain/Consilium.Domain.csproj src/Consilium.Domain/
COPY src/Consilium.Infrastructure/Consilium.Infrastructure.csproj src/Consilium.Infrastructure/

# Restore dependencies for the entire solution
RUN dotnet restore C.sln

# Copy the rest of the source code
# We copy the 'src' directory into the working directory (which is also '/src')
COPY ./src ./src

# Publish the API project (the runnable one)
RUN dotnet publish "src/Consilium.API/Consilium.API.csproj" -c Release -o /app/publish

# ===== STAGE 4: Backend Production (Updated) =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS build-backend-prod
WORKDIR /app
COPY --from=build-backend /app/publish .
EXPOSE 8080
# Update the entrypoint to the correct DLL name
ENTRYPOINT ["dotnet", "Consilium.API.dll"]