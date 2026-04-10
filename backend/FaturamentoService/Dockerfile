FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FaturamentoService.csproj", "./"]
RUN dotnet restore "FaturamentoService.csproj"
COPY . .
RUN dotnet build "FaturamentoService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FaturamentoService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FaturamentoService.dll"]
