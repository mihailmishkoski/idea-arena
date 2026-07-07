# syntax=docker/dockerfile:1

# ---- Stage 1: build the Angular frontend ----
FROM node:22-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---- Stage 2: build & publish the .NET app ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend
WORKDIR /src
COPY . .
RUN dotnet publish BusinessIdea/BusinessIdea.csproj -c Release -o /app/publish

# ---- Stage 3: runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=backend /app/publish ./
# Angular's compiled output becomes the API's static web root.
COPY --from=frontend /src/frontend/dist/frontend/browser ./wwwroot
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BusinessIdea.dll"]
