# La raíz de compilación debe ser la carpeta donde está NexaLearn.sln.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["NexaLearn/NexaLearn.csproj", "NexaLearn/"]
RUN dotnet restore "NexaLearn/NexaLearn.csproj"

COPY . .
RUN dotnet publish "NexaLearn/NexaLearn.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexaLearn.dll"]
