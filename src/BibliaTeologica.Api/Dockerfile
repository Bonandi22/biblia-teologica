# build a partir da raiz do repo: docker build -f src/BibliaTeologica.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/BibliaTeologica.Domain/*.csproj src/BibliaTeologica.Domain/
COPY src/BibliaTeologica.Application/*.csproj src/BibliaTeologica.Application/
COPY src/BibliaTeologica.Infrastructure/*.csproj src/BibliaTeologica.Infrastructure/
COPY src/BibliaTeologica.Api/*.csproj src/BibliaTeologica.Api/
RUN dotnet restore src/BibliaTeologica.Api/BibliaTeologica.Api.csproj

COPY src/ src/
COPY content/ content/
RUN dotnet publish src/BibliaTeologica.Api/BibliaTeologica.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
RUN mkdir -p /data && chown "$APP_UID" /data
COPY --from=build /app ./
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
ENTRYPOINT ["dotnet", "BibliaTeologica.Api.dll"]
