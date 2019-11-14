FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["BibliaVersosBot/BibliaVersosBot.csproj", "BibliaVersosBot/"]
RUN dotnet restore "BibliaVersosBot/BibliaVersosBot.csproj"
COPY . .
WORKDIR "/src/BibliaVersosBot"
RUN dotnet build "BibliaVersosBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BibliaVersosBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BibliaVersosBot.dll"]
