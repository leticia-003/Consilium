# ===== STAGE 1: Build Angular =====
FROM node:20 AS build-frontend
WORKDIR /app
COPY src/frontend/package*.json ./ 
RUN npm install
COPY src/frontend/ .
RUN npm run build

# ===== STAGE 2: Frontend Production (nginx) =====
FROM nginx:alpine AS build-frontend-prod
COPY --from=build-frontend /app/dist/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

# ===== STAGE 3: Build .NET backend =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src
COPY src/backend/ .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ===== STAGE 4: Backend Production =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS build-backend-prod
WORKDIR /app
COPY --from=build-backend /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "backend.dll"]
