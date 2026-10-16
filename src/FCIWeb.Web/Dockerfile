# Build multi-stage per FCIWeb.Web (.NET 8) — immagine runtime non-root.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Ripristino dipendenze (layer cache)
COPY src/Directory.Build.props src/Directory.Build.props
COPY src/FCIWeb.Core/FCIWeb.Core.csproj src/FCIWeb.Core/
COPY src/FCIWeb.Web/FCIWeb.Web.csproj src/FCIWeb.Web/
RUN dotnet restore src/FCIWeb.Web/FCIWeb.Web.csproj

# Build
COPY src/ src/
RUN dotnet publish src/FCIWeb.Web/FCIWeb.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "FCIWeb.Web.dll"]
