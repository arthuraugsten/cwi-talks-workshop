FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.4-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster as build
WORKDIR /src

COPY ["src/BuildingBlocks/NucleoCompartilhado/NucleoCompartilhado.csproj", "BuildingBlocks/NucleoCompartilhado/"]
COPY ["src/Servicos/Tarefas/Tarefas.csproj", "Servicos/Tarefas/"]

RUN dotnet restore "./Servicos/Tarefas/Tarefas.csproj"

COPY ["src/BuildingBlocks/NucleoCompartilhado/", "BuildingBlocks/NucleoCompartilhado/"]
COPY ["src/Servicos/Tarefas/", "Servicos/Tarefas/"]

WORKDIR /src/Servicos/Tarefas

FROM build AS publish
RUN dotnet publish "Tarefas.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "Tarefas.dll"]