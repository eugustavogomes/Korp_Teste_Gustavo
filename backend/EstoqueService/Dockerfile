FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS=80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EstoqueService.csproj", "./"]
RUN dotnet restore "EstoqueService.csproj"
COPY . .
RUN dotnet build "EstoqueService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EstoqueService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EstoqueService.dll"]
