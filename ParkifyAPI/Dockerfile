# Base image (runtime için)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image (build işlemi)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala
COPY ["ParkifyAPI/ParkifyAPI.csproj", "ParkifyAPI/"]
COPY ["ParkifyAPI.Common/ParkifyAPI.Common.csproj", "ParkifyAPI.Common/"]
COPY ["ParkifyAPI.Data/ParkifyAPI.Data.csproj", "ParkifyAPI.Data/"]
COPY ["ParkifyAPI.DataAccess/ParkifyAPI.DataAccess.csproj", "ParkifyAPI.DataAccess/"]

# Proje bağımlılıklarını restore et
RUN dotnet restore "ParkifyAPI/ParkifyAPI.csproj"

# Tüm proje kodlarını kopyala
COPY . .

# Build ve publish
WORKDIR "/src/ParkifyAPI"
RUN dotnet publish -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ParkifyAPI.dll"]
