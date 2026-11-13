# Build from the backend folder:  docker build -t stemdialogue-api .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props global.json StemDialogue.sln ./
COPY src/StemDialogue.Domain/StemDialogue.Domain.csproj src/StemDialogue.Domain/
COPY src/StemDialogue.Application/StemDialogue.Application.csproj src/StemDialogue.Application/
COPY src/StemDialogue.Infrastructure/StemDialogue.Infrastructure.csproj src/StemDialogue.Infrastructure/
COPY src/StemDialogue.Api/StemDialogue.Api.csproj src/StemDialogue.Api/
RUN dotnet restore src/StemDialogue.Api/StemDialogue.Api.csproj
COPY src/ src/
RUN dotnet publish src/StemDialogue.Api/StemDialogue.Api.csproj -c Release --no-restore -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out ./
RUN mkdir -p /app/App_Data/media && chown -R app:app /app/App_Data
ENV ASPNETCORE_HTTP_PORTS=8080 \
    Storage__RootPath=/app/App_Data/media
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "StemDialogue.Api.dll"]
