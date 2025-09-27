# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Instala fuentes necesarias en build
RUN apt-get update && apt-get install -y \
    fontconfig \
    libfreetype6 \
    && apt-get install -y ttf-mscorefonts-installer \
    && fc-cache -fv

# Copiar archivos de proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar carpeta Recursos
COPY Recursos ./Recursos

# Copiar todo el código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instala fuentes también en runtime
RUN apt-get update && apt-get install -y \
    fontconfig \
    libfreetype6 \
    && apt-get install -y ttf-mscorefonts-installer \
    && fc-cache -fv

COPY --from=build /app/out .

# Exponer puerto (Render usa el PORT de env variable)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProyDesaWeb2025.dll"]
