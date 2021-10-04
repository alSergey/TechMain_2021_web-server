FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /web-server
COPY . .
RUN dotnet build "TechMain_2021_web-server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TechMain_2021_web-server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY httptest ./httptest
ENTRYPOINT ["dotnet", "TechMain_2021_web-server.dll"]