# NexaLearn

API ASP.NET Core con .NET 9.

## Ejecutar la API de prueba

Desde la carpeta que contiene `NexaLearn.sln`:

```powershell
dotnet run --project NexaLearn/NexaLearn.csproj --launch-profile http
```

En Visual Studio, selecciona el perfil **http** y ejecuta el proyecto.
El navegador se abre en Swagger UI: <http://localhost:5025/swagger>.
Expande `GET /WeatherForecast`, pulsa **Try it out** y luego **Execute** para probar la API.
Este endpoint de ejemplo es público y devuelve cinco pronósticos sin consultar SQL Server.

El documento OpenAPI está disponible en <http://localhost:5025/openapi/v1.json>
en el entorno Development, al igual que Swagger UI.
También puedes ejecutar las solicitudes de `NexaLearn/NexaLearn.http`.

## HTTPS y autenticación

Para usar el perfil **https**, configura primero el certificado local:

```powershell
dotnet dev-certs https --trust
dotnet run --project NexaLearn/NexaLearn.csproj --launch-profile https
```

Con ese perfil, Swagger UI está en <https://localhost:7193/swagger>.

La autenticación JWT está registrada para los controladores que usen `[Authorize]`.
Para acceder a ellos debes configurar valores reales en `AzureAd` y enviar
un token válido. El permiso anónimo se limita al controlador de ejemplo
`WeatherForecastController`.
