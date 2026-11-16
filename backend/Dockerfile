# 1. Använd Microsofts officiella .NET SDK för att bygga appen
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Kopiera projektfilerna
COPY ["ShiftMate.Api/ShiftMate.Api.csproj", "ShiftMate.Api/"]
COPY ["ShiftMate.Application/ShiftMate.Application.csproj", "ShiftMate.Application/"]
COPY ["ShiftMate.Domain/ShiftMate.Domain.csproj", "ShiftMate.Domain/"]
COPY ["ShiftMate.Infrastructure/ShiftMate.Infrastructure.csproj", "ShiftMate.Infrastructure/"]

# 3. Hämta alla paket (Restore)
RUN dotnet restore "ShiftMate.Api/ShiftMate.Api.csproj"

# 4. Kopiera resten av koden och bygg
COPY . .
WORKDIR "/src/ShiftMate.Api"
RUN dotnet publish "ShiftMate.Api.csproj" -c Release -o /app/publish

# 5. Skapa den färdiga "bilden" som ska köras
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShiftMate.Api.dll"]