FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY build/ .
EXPOSE 8080
EXPOSE 8443
ENTRYPOINT ["dotnet", "SoftwareDeploymentProject.dll"]