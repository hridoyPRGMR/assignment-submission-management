# syntax=docker/dockerfile:1
# Build context is the `backend/` directory.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AssignmentSystem.Domain/AssignmentSystem.Domain.csproj src/AssignmentSystem.Domain/
COPY src/AssignmentSystem.Application/AssignmentSystem.Application.csproj src/AssignmentSystem.Application/
COPY src/AssignmentSystem.Infrastructure/AssignmentSystem.Infrastructure.csproj src/AssignmentSystem.Infrastructure/
COPY src/AssignmentSystem.Api/AssignmentSystem.Api.csproj src/AssignmentSystem.Api/
RUN dotnet restore src/AssignmentSystem.Api/AssignmentSystem.Api.csproj

COPY src/ src/
RUN dotnet publish src/AssignmentSystem.Api/AssignmentSystem.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssignmentSystem.Api.dll"]
