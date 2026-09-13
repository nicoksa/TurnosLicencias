# Turnos - Oficina de Licencias de Conducir

Aplicación de escritorio en C# / WinForms (.NET 8) para administrar turnos de la
Oficina de Licencias de Conducir de la Municipalidad. Pensada para uso local,
en una sola computadora, sin usuarios ni conexión a internet.

## Requisitos para compilar

- .NET 8 SDK instalado (https://dotnet.microsoft.com/download)
- Windows (WinForms solo corre en Windows)

## Cómo ejecutar

Desde la carpeta del proyecto:

```
dotnet restore
dotnet run
```

También se puede abrir `TurnosLicencias.csproj` directamente con Visual Studio 2022
(versión 17.8 o superior, con la carga de trabajo ".NET desktop development").

## Base de datos

- Se usa **SQLite** a través de **Entity Framework Core**.
- El archivo `turnos.db` se crea automáticamente la primera vez que se ejecuta
  la aplicación, en la misma carpeta que el ejecutable.
- No requiere instalar ningún motor de base de datos aparte: todo queda en ese
  único archivo, que se puede copiar/respaldar como cualquier archivo normal.

## Estructura del proyecto

```
TurnosLicencias/
├── TurnosLicencias.csproj
├── Program.cs                 -> Punto de entrada
├── Models/
│   └── Turno.cs                -> Entidad Turno
├── Data/
│   └── AppDbContext.cs         -> Contexto de EF Core / creación de la BD
├── Helpers/
│   └── HorarioHelper.cs        -> Genera la grilla de horarios (07:45 a 13:00, cada 15 min)
├── Services/
│   └── PrintService.cs         -> Impresión / vista previa del listado del día (A4)
└── Forms/
    ├── MainForm.cs              -> Pantalla principal (las dos columnas de oficinas)
    └── TurnoForm.cs             -> Alta / edición / eliminación de un turno
```

## Funcionalidad

- Selector de fecha en la parte superior.
- Dos columnas (Oficina 1 y Oficina 2), cada una con todos los horarios del día.
- Horarios libres en verde con la leyenda "LIBRE"; horarios ocupados en naranja
  con el nombre del ciudadano.
- Click en un horario libre → abre el formulario de alta de turno.
- Click en un horario ocupado → abre el mismo formulario en modo edición, con
  botón para eliminar (con confirmación).
- Botón "Próximo turno disponible" → busca, desde la fecha seleccionada (o desde
  hoy, lo que sea más adelante) hacia el futuro, el primer horario libre en
  cualquiera de las dos oficinas, salta la pantalla a esa fecha y ofrece
  cargarlo directamente.
- Botón "Imprimir listado del día" → genera un **archivo PDF real** (con
  PdfSharp) con los turnos de ambas oficinas —**Oficina 1 en una página y
  Oficina 2 en otra**, tabla con líneas de grilla, fuente grande, sin
  teléfono— y lo abre automáticamente con el lector de PDF predeterminado de
  Windows (Edge, Adobe Reader, etc.). Desde ahí el usuario imprime con el
  botón de impresión de su propio lector, como con cualquier PDF.
- Restricción a nivel de base de datos: no se puede reservar dos veces el
  mismo horario en la misma oficina (índice único por fecha + oficina + hora).

## Notas de diseño

- No hay login ni usuarios: la aplicación está pensada para un único puesto
  administrativo.
- La interfaz usa botones grandes y colores simples (verde = libre,
  naranja = ocupado) para que sea rápida de leer para personal administrativo.
- Toda la lógica de acceso a datos usa Entity Framework Core con el proveedor
  de SQLite, con `EnsureCreated()` (no requiere migraciones para este alcance).
