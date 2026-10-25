# e-Solutions - Plataforma de Pagos Multicanal (.NET / VB.NET)

Solución desarrollada como parte de la evaluación técnica para la posición de **Desarrollador .NET** en **e-Solutions, S.A.**  
Implementa una arquitectura web multicanal en **ASP.NET Web Forms** con **VB.NET**, base de datos en **SQL Server** y consumo asíncrono de servicios REST.

---

## 1. Arquitectura y Decisiones Técnicas

* **ASP.NET Web Forms (.NET Framework 4.8) con VB.NET:** Estructura modular orientada a separación de responsabilidades:
  * **Capa Web / Presentación:** Páginas (`Empresas.aspx`, `PagoEmpresa.aspx`, `DetalleEmpresa.aspx`, `Error.aspx`) y MasterPage (`Site.Master`) con diseño responsive mediante Bootstrap 5.
  * **Capa de Datos:** `ConfiguracionRepository` implementado con ADO.NET clásico y seguro. Uso de bloques `Using` para garantizar la liberación de conexiones al pool de SQL Server y comandos parametrizados contra inyección SQL.
  * **Capa de Servicios:** `EmpresasService` implementando `IEmpresasService` para consumo REST.
  * **Servicio Mock / Simulado:** `SimuladorEmpresas.ashx` (IHttpHandler) que emite respuestas JSON con simulación de latencia (`?delay=ms`), forzado de errores (`?error=1`) y validación obligatoria de `X-Api-Key`.
* **Consumo REST No Bloqueante:**
  * Uso de instancia `Shared` de `HttpClient` para prevenir agotamiento de sockets (*Socket Exhaustion*).
  * Soporte estricto de protocolos seguros modernos TLS 1.2 y TLS 1.3 en `Global.asax.vb`.
  * Integración con el ciclo de vida de Web Forms mediante `RegisterAsyncTask(New PageAsyncTask(...))` y llamadas con `.ConfigureAwait(False)` para evitar deadlocks con el `SynchronizationContext` de ASP.NET.
  * Timeouts granulares controlados por canal mediante `CancellationTokenSource`.
* **Comportamiento Multicanal Dinámico:**
  * La misma página (`Empresas.aspx`) se adapta al canal recibido en la URL (`?canal=AB` o `?canal=CJ`), leyendo su configuración desde la base de datos sin duplicar vistas ni código.
  * Control condicional del menú superior (`MuestraMenu`) y redirección a la página correspondiente (`PaginaDestino`) persistiendo la empresa seleccionada en `Session`.
  * Carga inicial estrictamente bajo `If Not IsPostBack` para evitar re-consultas innecesarias o pérdida de estado.
* **Pruebas Unitarias:**
  * Proyecto `eSolutions.Tests` con pruebas unitarias sobre `EmpresasService` desacopladas de la red mediante simulación de `HttpMessageHandler` (validando respuestas exitosas, 401 Unauthorized, errores 500 y validación de parámetros).

---

## 2. Requisitos Previos

* **Visual Studio 2022** (con las cargas de trabajo de desarrollo web y .NET Framework).
* **.NET Framework 4.8** o superior.
* **SQL Server** (SQL Server Express o SQL Server LocalDB `(localdb)\MSSQLLocalDB`).

---

## 3. Configuración de la Base de Datos

1. Abra una consola de comandos o SQL Server Management Studio (SSMS).
2. Conéctese a su instancia de SQL Server (por ejemplo, `(localdb)\MSSQLLocalDB`).
3. Ejecute el script ubicado en:
   ```text
   sql/Script_BaseDatos.sql
   ```
   Este script:
   * Crea la base de datos `eSolutionsPagos`.
   * Crea la tabla `CanalConfiguracion`.
   * Crea el procedimiento almacenado `dbo.sp_ObtenerConfiguracionCanal`.
   * Inserta los canales de prueba `AB` (Autoservicio Bancario) y `CJ` (Cajas y Ventanilla).

4. **Cadena de Conexión:**
   Verifique o modifique la cadena de conexión en el archivo `src/eSolutions.Web/Web.config`:
   ```xml
   <connectionStrings>
     <add name="eSolutionsConn" 
          connectionString="Server=(localdb)\MSSQLLocalDB;Database=eSolutionsPagos;Integrated Security=True;MultipleActiveResultSets=True;" 
          providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

---

## 4. Ejecución del Proyecto

1. Abra la solución `eSolutions.sln` en Visual Studio.
2. Establezca el proyecto `eSolutions.Web` como proyecto de inicio (*Set as Startup Project*).
3. Presione `Ctrl + F5` para iniciar IIS Express.

### Rutas de Prueba:

* **Canal AB (Autoservicio Bancario):**
  ```text
  http://localhost:[puerto]/Empresas.aspx?canal=AB
  ```
  * Muestra el menú de opciones superior.
  * Carga las empresas asociadas al canal AB.
  * Al hacer clic en "Pagar Servicio", navega a `PagoEmpresa.aspx` mostrando la empresa seleccionada.

* **Canal CJ (Cajas y Ventanilla):**
  ```text
  http://localhost:[puerto]/Empresas.aspx?canal=CJ
  ```
  * Oculta el menú superior (según configuración en BD).
  * Carga las empresas del canal CJ.
  * Al hacer clic en "Ver Detalle", navega a `DetalleEmpresa.aspx`.

* **Canal Inexistente o Inválido:**
  ```text
  http://localhost:[puerto]/Empresas.aspx?canal=NOEXISTE
  ```
  * Muestra panel de error controlado indicando que el canal no existe en catálogo.

---

## 5. Ejecución de Pruebas Unitarias

Desde Visual Studio:
* Menú **Prueba** -> **Ejecutar todas las pruebas** (`Ctrl + R, A`).
* O mediante la consola con `vstest.console.exe`:
  ```bash
  vstest.console.exe tests/eSolutions.Tests/bin/Debug/eSolutions.Tests.dll
  ```
  Resultado: 4/4 pruebas unitarias aprobadas.

---

## 6. Respuestas Teóricas (Parte A)

Las 8 preguntas teóricas solicitadas sobre ciclo de vida de Web Forms, persistencia de sesión en escenarios complejos, consumo de APIs, seguridad ADO.NET y optimizaciones se encuentran detalladas en el archivo:
* [Parte_A_Teorica.md](Parte_A_Teorica.md)
