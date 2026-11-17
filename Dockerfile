
# Compilacion de la aplicacion
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar los archivos de proyecto
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Web/Web.csproj src/Web/

# Restaurar dependencias
RUN dotnet restore src/Web/Web.csproj

# Copiar el codigo fuente
COPY . .

# Publicar la API
RUN dotnet publish src/Web/Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Imagen final para ejecucion
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

# Escuchar en el puerto proporcionado por Railway
ENTRYPOINT ["sh", "-c", "exec dotnet Web.dll --urls http://0.0.0.0:${PORT:-8080}"]
