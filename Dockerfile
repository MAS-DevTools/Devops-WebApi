# Multi-stage build: the SDK image compiles the app, the much smaller ASP.NET runtime image runs it.

# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer stays cached as long as the project file does not change.
COPY src/ProductApi/ProductApi.csproj src/ProductApi/
RUN dotnet restore src/ProductApi/ProductApi.csproj

COPY src/ src/
RUN dotnet publish src/ProductApi/ProductApi.csproj -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# The pipeline passes the Git commit SHA, so a running container shows which build it is.
ARG APP_VERSION=dev
ENV APP_VERSION=$APP_VERSION

COPY --from=build /app/publish .

# Run as the built-in non-root user of the .NET images.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductApi.dll"]
