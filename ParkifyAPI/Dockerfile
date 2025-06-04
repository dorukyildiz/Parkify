# 1. Build aşaması: .NET SDK kullanılır
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala (çoklu proje yapısı için)
COPY ["ParkifyAPI/ParkifyAPI.csproj", "ParkifyAPI/"]
COPY ["ParkifyAPI.Common/ParkifyAPI.Common.csproj", "ParkifyAPI.Common/"]
COPY ["ParkifyAPI.Data/ParkifyAPI.Data.csproj", "ParkifyAPI.Data/"]
COPY ["ParkifyAPI.DataAccess/ParkifyAPI.DataAccess.csproj", "ParkifyAPI.DataAccess/"]

# Bağımlılıkları restore et
RUN dotnet restore "ParkifyAPI/ParkifyAPI.csproj"

# Tüm kaynak kodunu kopyala
COPY . .

# Publish işlemini yap (Release konfigürasyonu)
WORKDIR "/src/ParkifyAPI"
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime aşaması: .NET runtime image kullanılır
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Build aşamasından çıkan publish edilen dosyaları kopyala
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "ParkifyAPI.dll"]
