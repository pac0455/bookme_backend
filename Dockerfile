# Usamos la imagen base para una aplicación ASP.NET Core (solo en tiempo de ejecución)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Usamos la imagen SDK para la construcción de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el código fuente al contenedor
COPY . .

# Restauramos las dependencias (esto puede tomar tiempo dependiendo de las dependencias)
RUN dotnet restore

# Publicamos la aplicación en la carpeta /app/publish
RUN dotnet publish -c Release -o /app/publish

# Usamos la imagen base para la etapa final
FROM base AS final
WORKDIR /app

# Copiamos los archivos publicados desde la etapa anterior
COPY --from=build /app/publish .

# Definimos el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "bookme_backend.dll"]
