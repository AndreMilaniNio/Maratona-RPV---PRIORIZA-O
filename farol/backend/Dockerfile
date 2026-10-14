# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY database/seed/ database/seed/
COPY backend/ backend/
RUN dotnet publish backend/src/Farol.API/Farol.API.csproj -c Release -o /app

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER app
# O Render injeta a porta em $PORT; localmente preservamos 8080 para o compose.
ENTRYPOINT ["sh", "-c", "exec dotnet Farol.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
