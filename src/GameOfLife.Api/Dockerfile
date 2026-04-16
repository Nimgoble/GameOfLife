# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/GameOfLife.Api/GameOfLife.Api.csproj", "src/GameOfLife.Api/"]
COPY ["src/GameOfLife.Core/GameOfLife.Core.csproj", "src/GameOfLife.Core/"]
COPY ["src/GameOfLife.Infrastructure/GameOfLife.Infrastructure.csproj", "src/GameOfLife.Infrastructure/"]
RUN dotnet restore "src/GameOfLife.Api/GameOfLife.Api.csproj"

# Copy remaining files and publish
COPY . .
WORKDIR "/src/src/GameOfLife.Api"
RUN dotnet publish "GameOfLife.Api.csproj" -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "GameOfLife.Api.dll"]
