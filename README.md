# ClientesApi

Web API RESTful robusta y escalable desarrollada con **ASP.NET Core (.NET 10)**, **Entity Framework Core**, **SQL Server** y seguridad basada en **JSON Web Tokens (JWT)** con cifrado de contraseñas mediante **BCrypt**. Incluye documentación interactiva integrada con **Scalar API Reference** y soporte para consumo de clientes de escritorio (`AppDemoClientes`).

---

## Tabla de Contenidos
- [Caracteristicas Principales](#caracteristicas-principales)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Base de Datos](#base-de-datos)
- [Configuracion y Puesta en Marcha](#configuracion-y-puesta-en-marcha)
- [Catalogo de Endpoints](#catalogo-de-endpoints)
- [Integracion con AppDemoClientes (Escritorio)](#integracion-con-appdemoclientes-escritorio)
- [Guia y Ejemplos de Pruebas](#guia-y-ejemplos-de-pruebas)

---

## Caracteristicas Principales

- **Autenticacion y Autorizacion JWT:** Emision de tokens de acceso seguros y validacion en cada solicitud protegida.
- **Seguridad con BCrypt:** Hasheo y verificacion de contraseñas sin almacenamiento en texto plano.
- **Control de Acceso Basado en Roles (RBAC):** Restricciones granulares (por ejemplo, eliminacion exclusiva para administradores `[Authorize(Roles = "Admin")]`).
- **Gestion de Clientes:** CRUD integral con validacion de unicidad de DNI, control de concurrencia y busqueda dinamica.
- **Gestion de Proveedores (Nuevo):** Mantenimiento completo con validacion de RUC unico (11 digitos), busqueda por razon social o RUC y actualizacion de datos de contacto.
- **Gestion de Productos:** Registro, actualizacion y consulta con validacion de numero de serie y precision monetaria.
- **Documentacion Interactiva:** Soporte nativo para OpenAPI y visor moderno con **Scalar UI**.

---

## Tecnologias Utilizadas

- **Framework:** .NET 10 (C# 13)
- **Persistencia / ORM:** Entity Framework Core 10 (SQL Server Provider)
- **Base de Datos:** Microsoft SQL Server
- **Seguridad:**
  - `Microsoft.AspNetCore.Authentication.JwtBearer` (v10.0.13)
  - `BCrypt.Net-Next` (v4.2.0)
  - `System.IdentityModel.Tokens.Jwt`
- **Documentacion API:** `Scalar.AspNetCore` (v2.18.0) y `Microsoft.AspNetCore.OpenApi`

---

## Estructura del Proyecto

```text
ClientesApi/
├── Controllers/
│   ├── AuthController.cs            # Endpoints de Registro y Login
│   ├── ClientesController.cs        # CRUD de Clientes con busqueda y proteccion JWT
│   ├── ProductosController.cs       # CRUD de Productos
│   └── ProveedoresController.cs     # CRUD de Proveedores con validacion de RUC
├── Data/
│   └── AppDbContext.cs               # Contexto EF Core y DbSets (Clientes, Productos, Usuarios, Proveedores)
├── Models/
│   ├── AuthDtos.cs                  # DTOs: RegistroRequest y LoginRequest
│   ├── Cliente.cs                   # Entidad Cliente (tabla clientes)
│   ├── Productos.cs                 # Entidad Productos (tabla productos)
│   ├── Proveedor.cs                 # Entidad Proveedor (tabla Proveedores - 5 campos)
│   └── Usuario.cs                   # Entidad Usuario (tabla Usuario)
├── appsettings.json                 # Cadena de conexion y configuracion JWT
└── Program.cs                       # Pipeline, inyeccion de dependencias y middlewares
```

---

## Base de Datos

El sistema se conecta a SQL Server mediante la base de datos `DemoClientes`. Las tablas principales son:

```sql
-- Tabla de Usuarios
CREATE TABLE Usuario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    passwordhash NVARCHAR(200) NOT NULL,
    rol NVARCHAR(30) NOT NULL DEFAULT 'Usuario'
);

-- Tabla de Clientes
CREATE TABLE clientes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    apellidos NVARCHAR(70) NOT NULL,
    nombres NVARCHAR(70) NOT NULL,
    telefono NVARCHAR(9) NOT NULL,
    correo NVARCHAR(70) NOT NULL,
    dni NVARCHAR(8) NOT NULL UNIQUE,
    direccion NVARCHAR(150) NOT NULL
);

-- Tabla de Productos
CREATE TABLE productos (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(100) NOT NULL,
    precio DECIMAL(7,2) NOT NULL,
    garantia TINYINT NOT NULL,
    numserie NVARCHAR(40) NULL
);

-- Tabla de Proveedores (Nueva)
CREATE TABLE Proveedores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RazonSocial VARCHAR(120) NOT NULL,
    Ruc VARCHAR(11) NOT NULL UNIQUE,
    Telefono VARCHAR(15) NULL,
    Correo VARCHAR(100) NULL
);
```

---

## Configuracion y Puesta en Marcha

### 1. Clonar el repositorio
```bash
git clone https://github.com/Pineda-25/ClientesApi.git
cd ClientesApi
```

### 2. Configurar `appsettings.json`
Verifica la cadena de conexion a tu instancia de SQL Server y los parametros JWT:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DemoClientes;User Id=sa;Password=tu_password;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "IngresaUnaClaveDeNoMenos32CaracteresProtecionAPppWebJWT!2026.",
    "Issuer": "CLientesApi",
    "Audience": "ClientesApiUsuarios",
    "ExpireMinutes": 60
  }
}
```

### 3. Restaurar dependencias y ejecutar
```bash
dotnet restore
dotnet build
dotnet run --project ClientesApi/ClientesApi.csproj
```

La API iniciara en `https://localhost:7177` (o en el puerto asignado en tu `launchSettings.json`). Puedes acceder a la documentacion interactiva de Scalar en:
`https://localhost:7177/scalar/v1`

---

## Catalogo de Endpoints

### Autenticacion (`/api/auth`)
| Metodo | Endpoint | Descripcion | Acceso |
|---|---|---|---|
| `POST` | `/api/auth/registrar` | Registra un nuevo usuario con contraseña cifrada (BCrypt) | Publico |
| `POST` | `/api/auth/login` | Valida credenciales y genera el Bearer Token JWT | Publico |

### Clientes (`/api/clientes`)
| Metodo | Endpoint | Descripcion | Acceso |
|---|---|---|---|
| `GET` | `/api/clientes` | Obtiene listado de clientes (soporta `?buscar=...` por nombres, apellidos o DNI) | Requiere JWT |
| `GET` | `/api/clientes/{id}` | Busca y retorna un cliente por su ID | Requiere JWT |
| `POST` | `/api/clientes` | Registra un nuevo cliente (valida DNI duplicado) | Requiere JWT |
| `PUT` | `/api/clientes/{id}` | Actualiza los datos de un cliente existente | Requiere JWT |
| `DELETE` | `/api/clientes/{id}` | Elimina un cliente por su ID | Requiere rol **Admin** |

### Proveedores (`/api/proveedores`) - Nuevo
| Metodo | Endpoint | Descripcion | Acceso |
|---|---|---|---|
| `GET` | `/api/proveedores` | Lista proveedores con filtro opcional `?buscar=...` por razon social o RUC | Publico |
| `GET` | `/api/proveedores/{id}` | Obtiene el detalle de un proveedor por su ID | Publico |
| `POST` | `/api/proveedores` | Registra un proveedor (valida unicidad de RUC de 11 digitos) | Publico |
| `PUT` | `/api/proveedores/{id}` | Actualiza datos de contacto y razon social del proveedor | Publico |
| `DELETE` | `/api/proveedores/{id}` | Elimina un proveedor por su ID | Publico |

### Productos (`/api/productos`)
| Metodo | Endpoint | Descripcion | Acceso |
|---|---|---|---|
| `GET` | `/api/productos` | Obtiene el catalogo de productos | Publico |
| `GET` | `/api/productos/{id}` | Obtiene el detalle de un producto por ID | Publico |
| `POST` | `/api/productos` | Registra un nuevo producto (valida serie duplicada) | Publico |
| `PUT` | `/api/productos/{id}` | Actualiza los datos del producto | Publico |
| `DELETE` | `/api/productos/{id}` | Elimina un producto por su ID | Publico |

---

## Integracion con AppDemoClientes (Escritorio)

Este backend está diseñado para conectarse y alimentar a la aplicacion de escritorio **AppDemoClientes** (Windows Forms estructurada en capas: `UI`, `BOL`, `DAL`, `Entidades`):

1. **Flujo de navegacion:** Se implementa un formulario contenedor principal `FrmMain` que dispone de acceso a los dos modulos del sistema:
   - **Clientes (`FrmCliente`):** Gestion de cartera de clientes.
   - **Proveedores (`FrmProveedor`):** Mantenimiento CRUD completo de empresas proveedoras.
2. **Arquitectura desacoplada:**
   - La capa `DAL` (`ProveedorDal`) consume los endpoints REST (`/api/proveedores`).
   - La capa `BOL` (`ProveedorBol`) procesa las reglas de negocio, normaliza datos y valida la estructura de RUC (11 digitos numéricos) y campos obligatorios.
   - La capa `UI` (`FrmProveedor`) expone la interfaz grafica reactiva y asincrona con DataGridView, orden de tabulacion optimizado y controles de validacion en tiempo real.

---

## Guia y Ejemplos de Pruebas

Registro detallado de pruebas realizadas para validar el flujo completo:

```text
*** Test del Modulo Proveedores (CRUD) ***

1. Registrar un Proveedor:
POST - https://localhost:7177/api/proveedores
Body (JSON):
{
  "razonSocial": "Distribuidora Textil Perú SAC",
  "ruc": "20458796321",
  "telefono": "987654321",
  "correo": "ventas@textilperu.com"
}
Resultado: 201 Created -> { "id": 1, "razonSocial": "...", ... }

2. Validacion de RUC duplicado:
POST - https://localhost:7177/api/proveedores (mismo RUC)
Resultado: 409 Conflict -> { "mensaje": "Ya existe un proveedor con ese RUC" }

3. Busqueda con filtro:
GET - https://localhost:7177/api/proveedores?buscar=Textil
Resultado: 200 OK -> Listado con los registros coincidentes.

4. Actualizacion:
PUT - https://localhost:7177/api/proveedores/1
Body (JSON):
{
  "id": 1,
  "razonSocial": "Distribuidora Textil Perú SAC",
  "ruc": "20458796321",
  "telefono": "999888777",
  "correo": "contacto@textilperu.com"
}
Resultado: 204 No Content
```
