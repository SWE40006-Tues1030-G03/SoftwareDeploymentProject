FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY build/ .
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "SoftwareDeploymentProject.dll"]