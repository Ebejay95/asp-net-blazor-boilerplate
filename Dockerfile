FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files in correct order (dependencies first)
COPY ["src/CMC.Domain/CMC.Domain.csproj", "src/CMC.Domain/"]
COPY ["src/CMC.Contracts/CMC.Contracts.csproj", "src/CMC.Contracts/"]
COPY ["src/CMC.Application/CMC.Application.csproj", "src/CMC.Application/"]
COPY ["src/CMC.Infrastructure/CMC.Infrastructure.csproj", "src/CMC.Infrastructure/"]
COPY ["src/CMC.Web/CMC.Web.csproj", "src/CMC.Web/"]

# Restore dependencies
RUN dotnet restore "src/CMC.Web/CMC.Web.csproj"

# Copy source code (including pre-built migrations)
COPY . .

# Build the application
WORKDIR "/src/src/CMC.Web"
RUN dotnet build "CMC.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CMC.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "CMC.Web.dll"]
