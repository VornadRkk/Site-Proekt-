FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/DateAndTime.Api/DateAndTime.Api.csproj", "backend/DateAndTime.Api/"]
RUN dotnet restore "backend/DateAndTime.Api/DateAndTime.Api.csproj"
COPY . .
WORKDIR "/src/backend/DateAndTime.Api"
RUN dotnet publish "DateAndTime.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DateAndTime.Api.dll"]
