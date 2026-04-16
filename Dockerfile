# ============================================================
# Stage 1: Build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (layer-cache friendly)
COPY ["src/GameOfLife.Api/GameOfLife.Api.csproj",            "src/GameOfLife.Api/"]
COPY ["src/GameOfLife.Core/GameOfLife.Core.csproj",          "src/GameOfLife.Core/"]
COPY ["src/GameOfLife.Infrastructure/GameOfLife.Infrastructure.csproj", "src/GameOfLife.Infrastructure/"]

RUN dotnet restore "src/GameOfLife.Api/GameOfLife.Api.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/src/GameOfLife.Api"
RUN dotnet publish "GameOfLife.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============================================================
# Stage 2: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# Persist SQLite DB outside the container image
VOLUME ["/app/data"]
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/gameoflife.db"

COPY --from=build /app/publish .

USER appuser
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameOfLife.Api.dll"]
