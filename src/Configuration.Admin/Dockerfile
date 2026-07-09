FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore
COPY ["src/Configuration.Admin/Configuration.Admin.csproj", "Configuration.Admin/"]
COPY ["src/Configuration.Domain/Configuration.Domain.csproj", "Configuration.Domain/"]
COPY ["src/Configuration.Application/Configuration.Application.csproj", "Configuration.Application/"]
COPY ["src/Configuration.Infrastructure/Configuration.Infrastructure.csproj", "Configuration.Infrastructure/"]
COPY ["src/Configuration.Library/Configuration.Library.csproj", "Configuration.Library/"]
COPY ["src/Configuration.Shared/Configuration.Shared.csproj", "Configuration.Shared/"]
RUN dotnet restore "Configuration.Admin/Configuration.Admin.csproj"

# Copy source files and build
COPY src/Configuration.Domain/ Configuration.Domain/
COPY src/Configuration.Application/ Configuration.Application/
COPY src/Configuration.Infrastructure/ Configuration.Infrastructure/
COPY src/Configuration.Library/ Configuration.Library/
COPY src/Configuration.Shared/ Configuration.Shared/
COPY src/Configuration.Admin/ Configuration.Admin/
RUN dotnet build "Configuration.Admin/Configuration.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Configuration.Admin/Configuration.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Configuration.Admin.dll"]
