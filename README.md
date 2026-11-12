# NexaLearn

NexaLearn es una plataforma de cursos en línea. Permite registrarse, iniciar sesión, ver cursos, comprar con PayPal y acceder a los videos después de confirmar el pago. También incluye un panel para que el administrador gestione cursos, lecciones, pagos e inscripciones.

## Proyectos

Este repositorio contiene la API. El frontend está en la carpeta hermana `nexalearn-frontend`.

| Proyecto | Tecnología | Dirección local |
| --- | --- | --- |
| API | ASP.NET Core 9 | `https://localhost:7193` |
| Frontend | React + Vite | `http://localhost:5173` |

## Requisitos

- .NET SDK 9
- Node.js 20 o superior
- SQL Server

## Ejecutar de forma local

### 1. Configurar la API

En `NexaLearn/appsettings.json` completa los valores necesarios para tu base de datos, JWT y correo. También puedes usar `user-secrets` para las claves privadas.

Valores mínimos:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "TU_CADENA_DE_SQL_SERVER"
  },
  "Jwt": {
    "Key": "UNA_CLAVE_DE_AL_MENOS_32_CARACTERES"
  },
  "Identity": {
    "AdministradorInicial": {
      "Email": "admin@ejemplo.com",
      "Password": "UnaClaveSegura2026"
    }
  }
}
```

### 2. Preparar la base de datos

1. Crea una base de datos vacía llamada `NexaLearn` en SQL Server.
2. Aplica las migraciones de Identity:

```powershell
dotnet ef database update --project .\NexaLearn\NexaLearn.csproj --context ApplicationIdentityDbContext
```

3. Abre `NexaLearn/NexaLearn.sql` con SQL Server Management Studio y ejecútalo sobre la base de datos `NexaLearn`.

Este orden es importante: primero Identity y después el script SQL de cursos, categorías, pagos e inscripciones.

Para PayPal Sandbox agrega:

```powershell
dotnet user-secrets set "PayPal:ClientId" "TU_CLIENT_ID" --project .\NexaLearn\NexaLearn.csproj
dotnet user-secrets set "PayPal:ClientSecret" "TU_CLIENT_SECRET" --project .\NexaLearn\NexaLearn.csproj
dotnet user-secrets set "PayPal:ReturnUrl" "http://localhost:5173/pago/confirmado" --project .\NexaLearn\NexaLearn.csproj
dotnet user-secrets set "PayPal:CancelUrl" "http://localhost:5173/home" --project .\NexaLearn\NexaLearn.csproj
```

Confía el certificado HTTPS local y ejecuta la API:

```powershell
dotnet dev-certs https --trust
dotnet run --project .\NexaLearn\NexaLearn.csproj --launch-profile https
```

La documentación estará disponible en:

```text
https://localhost:7193/docs
```



## Usuarios y roles

- `Admin`: crea cursos y lecciones; revisa pagos e inscripciones.
- `Cliente`: compra cursos y puede ver los videos de los cursos con inscripción activa.

El usuario administrador se crea al iniciar la API si llenas `Identity:AdministradorInicial`.

## Carpetas principales de la API

| Carpeta | Uso |
| --- | --- |
| `Controllers` | Endpoints de la API. |
| `DTOs` | Datos que entran y salen de los endpoints. |
| `Repositories` | Consultas y guardado de datos. |
| `Services` | Acciones como pagos PayPal, correo e imágenes. |
| `Models` | Entidades y conexión con la base de datos. |
| `Identity` | Usuarios, roles y autenticación. |
| `Datatable` | Paginación de listados. |
| `Infrastructure` | Manejo general de errores. |
| `Templates` | Plantilla HTML usada para correos. |
| `Properties` | Configuración para iniciar el proyecto localmente. |

## Publicar con Docker

La API usa el `Dockerfile` de este repositorio. El frontend tiene su propio `Dockerfile` dentro de `nexalearn-frontend`.

En producción configura estas variables:

```env

# API
Cors__FrontendUrl=https://tu-frontend.com
PayPal__ClientId=TU_CLIENT_ID
PayPal__ClientSecret=TU_CLIENT_SECRET
PayPal__Currency=USD
PayPal__ReturnUrl=https://tu-frontend.com/pago/confirmado
PayPal__CancelUrl=https://tu-frontend.com/home
```
