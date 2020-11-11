FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.4-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as build
WORKDIR /src

COPY ["src/BuildingBlocks/NucleoCompartilhado/NucleoCompartilhado.csproj", "BuildingBlocks/NucleoCompartilhado/"]
COPY ["src/Servicos/Usuarios/Usuarios.csproj", "Servicos/Usuarios/"]

RUN dotnet restore "./Servicos/Usuarios/Usuarios.csproj"

COPY ["src/BuildingBlocks/NucleoCompartilhado/", "BuildingBlocks/NucleoCompartilhado/"]
COPY ["src/Servicos/Usuarios/", "Servicos/Usuarios/"]

WORKDIR /src/Servicos/Usuarios

FROM build AS publish
RUN dotnet publish "Usuarios.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "Usuarios.dll"]