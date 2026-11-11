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

La autenticación JWT protege las rutas que usan `[Authorize]`.

## Identity, JWT y roles

La API usa ASP.NET Core Identity local y JWT. Los roles son `Admin` y `Cliente`.
Al iniciar, se crean los dos roles si aún no existen. El registro público crea
usuarios con el rol `Cliente`; el usuario no puede elegir su propio rol.

Completa directamente `NexaLearn/appsettings.json`. Todas las propiedades ya
están creadas y vacías: `ConnectionStrings:DefaultConnection`, `Jwt:Key`,
`Identity:AdministradorInicial:Email` e
`Identity:AdministradorInicial:Password`.

`Jwt:Key` debe tener al menos 32 caracteres. Para generar una clave compatible
con la consola de Visual Studio:

```powershell
$jwtKeyBytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($jwtKeyBytes)
$rng.Dispose()
[Convert]::ToBase64String($jwtKeyBytes)
```

Copia el resultado en `Jwt:Key`. Al iniciar, si el correo de
`AdministradorInicial` no existe, se crea y recibe el rol `Admin`. Si ya existe,
se le asigna ese rol.

La base de datos necesita las tablas de Identity, que ya tienen una migración
en el proyecto. Aplícala una sola vez contra tu base de desarrollo:

```powershell
dotnet ef database update --project NexaLearn/NexaLearn.csproj --context ApplicationIdentityDbContext
```

| Método | Ruta | Acceso |
| --- | --- | --- |
| POST | `/api/auth/registro` | Público; crea un Cliente |
| POST | `/api/auth/login` | Público; devuelve JWT y roles |
| GET | `/api/auth/actual` | JWT válido |
| PUT | `/api/auth/usuarios/{userId}/rol` | Solo Admin |

Para enviar el JWT fuera de Swagger usa el encabezado:

```http
Authorization: Bearer tu_token
```

Swagger muestra el botón **Authorize**. Pega solo el token, sin escribir
`Bearer`. Las consultas GET del catálogo de categorías, cursos, lecciones e
imágenes son públicas. Crear, actualizar o eliminar categorías, cursos,
lecciones e imágenes requiere `Admin`. Inscripciones, pagos y perfiles también
son rutas administrativas y requieren `Admin`. Un Cliente recibe HTTP 403 al
intentarlas y una solicitud sin token recibe HTTP 401.

## Configuración de correo

Inyecta `ICorreoService` y usa un único método:

```csharp
await correoService.EnviarAsync(
    "destino@ejemplo.com",
    "Inscripción confirmada",
    "Hola Ana, tu inscripción fue registrada.",
    cancellationToken);
```

Completa la sección `Smtp` de `NexaLearn/appsettings.json`:

```json
"Smtp": {
  "Host": "smtp.tuproveedor.com",
  "Puerto": 587,
  "UsarSsl": true,
  "Remitente": "notificaciones@tudominio.com",
  "Usuario": "tu-usuario",
  "Clave": "tu-clave-o-contrasena-de-aplicacion"
}
```

La plantilla está en
`NexaLearn/Templates/Correo.html`; `{0}` es el asunto y `{1}` el contenido.

Los correos se intentan enviar después de estas operaciones ya guardadas:

- Registro de usuario: mensaje de bienvenida.
- Creación de inscripción: confirmación con el título del curso.
- Pago creado con estado `PAGADO`, o pago actualizado desde otro estado a `PAGADO`: confirmación del pago.

Si SMTP no está configurado o el envío falla, la operación de usuario,
inscripción o pago no se revierte; el error queda registrado en los logs de la API.
