FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY .editorconfig Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/ShortLink.Domain/ShortLink.Domain.csproj src/ShortLink.Domain/
COPY src/ShortLink.Application/ShortLink.Application.csproj src/ShortLink.Application/
COPY src/ShortLink.Infrastructure/ShortLink.Infrastructure.csproj src/ShortLink.Infrastructure/
COPY src/ShortLink.Api/ShortLink.Api.csproj src/ShortLink.Api/
RUN dotnet restore src/ShortLink.Api/ShortLink.Api.csproj

COPY src/ src/
RUN dotnet publish src/ShortLink.Api/ShortLink.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ShortLink.Api.dll"]