# Usa la imagen SDK para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia el archivo de proyecto y restaura las dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto de los archivos de la aplicación
COPY . ./

# Publica la aplicación en modo Release
RUN dotnet publish -c Release -o out

# Usa una imagen de runtime más ligera para la aplicación final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Expone el puerto 8080 (Varnish se conectará a este)
EXPOSE 8080

# Comando de inicio de la aplicación
ENTRYPOINT ["dotnet", "CachePoisoningLab.dll"]