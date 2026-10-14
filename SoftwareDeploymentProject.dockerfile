FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY build/ .
ENTRYPOINT ["dotnet", "SoftwareDeploymentProject.dll"]