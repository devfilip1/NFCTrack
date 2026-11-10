FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY .editorconfig Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/ShortLink.Domain/ShortLink.Domain.csproj src/ShortLink.Domain/
COPY src/ShortLink.Application/ShortLink.Application.csproj src/ShortLink.Application/
COPY src/ShortLink.Contracts/ShortLink.Contracts.csproj src/ShortLink.Contracts/
COPY src/ShortLink.Infrastructure/ShortLink.Infrastructure.csproj src/ShortLink.Infrastructure/
COPY src/ShortLink.Worker/ShortLink.Worker.csproj src/ShortLink.Worker/
RUN dotnet restore src/ShortLink.Worker/ShortLink.Worker.csproj

COPY src/ src/
RUN dotnet publish src/ShortLink.Worker/ShortLink.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "ShortLink.Worker.dll"]
