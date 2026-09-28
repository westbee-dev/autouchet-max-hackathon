FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AutoUchet.Api/AutoUchet.Api.csproj", "AutoUchet.Api/"]
RUN dotnet restore "AutoUchet.Api/AutoUchet.Api.csproj"

COPY . .
WORKDIR "/src/AutoUchet.Api"
RUN dotnet publish "AutoUchet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AutoUchet.Api.dll"]