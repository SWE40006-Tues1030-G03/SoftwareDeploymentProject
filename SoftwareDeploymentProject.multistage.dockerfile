
# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY SoftwareDeploymentProject/SoftwareDeploymentProject.csproj SoftwareDeploymentProject/
RUN dotnet restore SoftwareDeploymentProject/SoftwareDeploymentProject.csproj
COPY SoftwareDeploymentProject/ SoftwareDeploymentProject/

WORKDIR /src/SoftwareDeploymentProject
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SoftwareDeploymentProject.dll"]
