FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Kevin.ApiService/Kevin.ApiService.csproj Kevin.ApiService/
COPY Kevin.ServiceDefaults/Kevin.ServiceDefaults.csproj Kevin.ServiceDefaults/
RUN dotnet restore Kevin.ApiService/Kevin.ApiService.csproj
COPY Kevin.ApiService/ Kevin.ApiService/
COPY Kevin.ServiceDefaults/ Kevin.ServiceDefaults/
RUN dotnet publish Kevin.ApiService/Kevin.ApiService.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Kevin.ApiService.dll"]
