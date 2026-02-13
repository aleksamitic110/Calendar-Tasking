FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["CalendarTasking.sln", "./"]
COPY ["src/CalendarTasking.Api/CalendarTasking.Api.csproj", "src/CalendarTasking.Api/"]
RUN dotnet restore "src/CalendarTasking.Api/CalendarTasking.Api.csproj"

COPY ["src/CalendarTasking.Api/", "src/CalendarTasking.Api/"]
COPY ["client/", "client/"]

RUN dotnet publish "src/CalendarTasking.Api/CalendarTasking.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
COPY --from=build /src/client /client

ENTRYPOINT ["dotnet", "CalendarTasking.Api.dll"]
