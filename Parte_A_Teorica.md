# Evaluacion Tecnica - Desarrollador .NET
## Parte A: Preguntas Teoricas

**Candidato:** Jonathan  
**Posicion:** Desarrollador .NET  

---

### 1. Ciclo de vida de una pagina Web Forms
**Explica el ciclo de vida de una pagina Web Forms (Init, Load, eventos de controles, PreRender, Render). ¿En que evento cargas los datos y que pasa si no verificas `IsPostBack`?**

**Respuesta:**
El ciclo de vida en Web Forms maneja la inicializacion, el estado y el dibujado de los controles en el siguiente orden basico:
1. **Init (`Page_Init`):** Se inicializan los controles del arbol de la pagina. Aqui todavia no se ha cargado el ViewState ni la informacion que envio el usuario.
2. **Load (`Page_Load`):** Los controles ya recuperaron su estado previo (ViewState) y los datos que envio el usuario por POST. Es el lugar principal para inicializar la logica y cargar datos.
3. **Eventos de controles:** Si el usuario dio clic en un boton o cambio un combo con autopostback, aqui se ejecutan esos eventos (como `btnGuardar_Click` o `rpt_ItemCommand`).
4. **PreRender:** Es el ultimo momento antes del renderizado donde todavia podemos modificar propiedades visuales de los controles (por ejemplo ocultar un panel o cambiar un texto).
5. **Render:** ASP.NET genera el HTML final que se le envia al navegador.

**Carga de datos e IsPostBack:**
Los datos se deben cargar en el evento **`Page_Load`**, pero siempre dentro de la condicion:
```vb
If Not IsPostBack Then
    CargarDatos()
End If
```
Si se olvida validar `IsPostBack`, cada vez que el usuario interactue con la pagina (un clic en un boton o paginacion) la pagina volvera a consultar la base de datos y a re-enlazar los controles (`DataBind`). Esto causa:
* Se sobreescriben los datos y cambios que el usuario haya hecho en la pantalla antes de que se ejecute el evento del boton.
* Puede hacer que no se disparen correctamente los eventos de controles como botones o listas.
* Se desperdician recursos del servidor y la red haciendo consultas innecesarias en cada clic.

---

### 2. ViewState, Session y Query String
**Diferencia entre ViewState, Session y query string. ¿Cuando usarias cada uno y que riesgos tienen (tamaño, seguridad, manipulacion)?**

**Respuesta:**

* **ViewState:**
  * **Que es:** Es un mecanismo a nivel de pagina que viaja oculto en el HTML del cliente en el campo `__VIEWSTATE` en Base64.
  * **Uso:** Lo uso para mantener el estado visual de los controles de la misma pagina entre postbacks (por ejemplo para saber que pagina de un grid estaba viendo o mantener una lista en memoria de la pagina).
  * **Riesgos:** Si se guardan tablas grandes hace que el HTML pese mucho (ViewState bloat) y la pagina se ponga lenta al cargar. En seguridad, Base64 no es cifrado, por lo que cualquiera puede decodificarlo si no se activa la validacion MAC y cifrado en el servidor.

* **Session:**
  * **Que es:** Memoria en el servidor asociada a un usuario especifico mediante una cookie (`ASP.NET_SessionId`).
  * **Uso:** Para compartir informacion del usuario entre distintas paginas durante su navegacion (por ejemplo datos del usuario autenticado, el carrito de compras o la empresa que selecciono para pagar).
  * **Riesgos:** En modo por defecto (InProc) consume memoria RAM del servidor web. Si hay miles de usuarios concurrentes puede agotar la memoria. Ademas, si el proceso de IIS se reinicia se pierde la sesion si no se usa SQL Server o un servidor de estado.

* **Query String:**
  * **Que es:** Parametros que viajan directamente en la URL despues del signo `?` (ej: `?canal=AB&id=10`).
  * **Uso:** Para navegacion directa, filtros sencillos o cuando se necesita compartir un enlace directo a una vista especifica (como el canal de atencion).
  * **Riesgos:** Esta a la vista de todos en la barra de direcciones y queda en el historial del navegador. Nunca se deben pasar contraseñas ni datos sensibles por ahi, y siempre hay que validar y sanitizar lo que venga en la URL porque el usuario lo puede cambiar manualmente con facilidad.

---

### 3. Problemas de sesion en entornos complejos
**¿Que problemas de sesion pueden aparecer con varios servidores, con el reciclaje del application pool o cuando la pagina se abre dentro de un iframe de otro dominio o en Safari (iOS)? ¿Como los resolverias?**

**Respuesta:**

1. **Varios servidores (Balanceador / Web Farm):**  
   Si se usa el modo normal en memoria (`InProc`), la sesion vive solo en el servidor que atendio la primera peticion. Si la siguiente peticion cae en otro nodo del balanceador, el sistema no encuentra la sesion y saca al usuario.  
   *Solucion:* Sacar la sesion fuera de los servidores web usando **SQL Server Session State** o **Redis Cache**. Tambien se puede activar afinidad de sesiones (Sticky Sessions) en el balanceador, aunque usar Redis/SQL es mejor porque si un servidor se apaga no se pierde la sesion. Todos los objetos en sesion deben tener `<Serializable>`.

2. **Reciclaje del Application Pool de IIS:**  
   Cuando IIS recicla el pool (por consumo de memoria, horarios programados o cambios en la app), el proceso `w3wp.exe` se reinicia y borra toda la memoria RAM, perdiendo las sesiones de los usuarios.  
   *Solucion:* Igual que el punto anterior, guardar la sesion de forma distribuida en SQL Server o Redis para que los reinicios de IIS sean invisibles para el usuario.

3. **Iframes entre dominios y Safari (iOS / ITP):**  
   Safari con su politica de privacidad ITP y los navegadores actuales bloquean las cookies de terceros por defecto. Si nuestro sitio se abre dentro de un iframe que esta alojado en otro dominio (por ejemplo la web de un banco), el navegador no envia la cookie `ASP.NET_SessionId` y la sesion se pierde en cada clic.  
   *Solucion:* Configurar las cookies de sesion con `SameSite=None` y `Secure` bajo HTTPS en el `web.config`. Para evitar depender de cookies en iframes, lo mas recomendable en la practica es pasar tokens de autorizacion efimeros por URL/cabeceras o comunicar la ventana padre con el iframe mediante `window.postMessage`.

---

### 4. Consumo de API REST desde .NET Framework
**¿Como consumes una API REST desde .NET Framework: reutilizacion de `HttpClient`, TLS 1.2, timeouts y por que usar `.Result` o `.Wait()` puede bloquear una pagina?**

**Respuesta:**

* **Reutilizacion de HttpClient:**  
  No se debe instanciar `HttpClient` dentro de un bloque `Using` en cada peticion. Aunque tenga Dispose, el socket de red se queda abierto en el sistema operativo en estado `TIME_WAIT` por un par de minutos. Si hay trafico alto, se acaban los puertos disponibles (Socket Exhaustion) y empiezan a fallar las conexiones. Lo correcto es manejar una sola instancia compartida (`Shared` / static) para toda la aplicacion.

* **TLS 1.2 / 1.3:**  
  En versiones de .NET Framework anteriores a la 4.7/4.8, el protocolo por defecto solia ser SSL3 o TLS 1.0, lo que causa errores de conexion con APIs modernas. Hay que forzarlo en el inicio de la app (`Application_Start` en Global.asax) con:
  ```vb
  ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls13
  ```

* **Manejo de Timeouts:**  
  Es una mala practica dejar que una peticion espere indefinidamente si el servicio remoto esta caido. Se debe configurar un timeout razonable y usar un `CancellationTokenSource` con tiempo limite en el metodo `SendAsync` para cancelar la tarea y mostrar un mensaje controlado de demora al usuario.

* **Peligro de `.Result` o `.Wait()` (Deadlocks):**  
  En ASP.NET clasico existe un contexto de sincronizacion (`AspNetSynchronizationContext`) que asegura que la peticion se ejecute en un solo hilo. Si desde ese hilo llamas a un metodo asincrono y pones `.Result` o `.Wait()`, el hilo se queda bloqueado esperando. Cuando la llamada HTTP remota responde, intenta regresar al hilo original para continuar, pero como ese hilo esta ocupado esperandola a ella misma, ninguno avanza y se genera un **deadlock**. La pagina se cuelga hasta que truena por timeout.  
  Por eso se debe usar asincronismo real de punta a punta con `RegisterAsyncTask` en la pagina y `.ConfigureAwait(False)` en los servicios.

---

### 5. Invocacion segura de Stored Procedures con ADO.NET
**¿Como invocas un procedimiento almacenado de SQL Server con ADO.NET de forma segura: parametros, manejo de conexiones con `Using` y transacciones?**

**Respuesta:**

Para hacer una llamada segura y limpia con ADO.NET tomo en cuenta tres cosas:

1. **Parametros tipados:** Siempre usar `cmd.Parameters.Add` indicando el tipo (`SqlDbType`) y valor del parametro, jamas concatenar variables dentro de texto sql. Esto elimina el riesgo de inyeccion SQL y permite que SQL Server reutilice los planes de ejecucion en cache.
2. **Uso de bloques `Using`:** Usar `Using conn As New SqlConnection(...)` y `Using cmd As New SqlCommand(...)`. Esto asegura que al terminar o si se produce un error, se ejecute el `Dispose()` y la conexion se cierre inmediatamente, regresando al pool de conexiones sin dejarlas abiertas.
3. **Manejo de Transacciones:** Si se van a ejecutar varios inserts o updates que dependen entre si, se inicia una transaccion con `conn.BeginTransaction()`. Todo se mete en un `Try...Catch`; si todo sale bien se hace `tx.Commit()` y si salta alguna excepcion se hace `tx.Rollback()` en el catch para no dejar datos a medias.

Ejemplo practico:
```vb
Using conn As New SqlConnection(cadenaConexion)
    conn.Open()
    Using tx = conn.BeginTransaction()
        Try
            Using cmd As New SqlCommand("dbo.sp_GuardarOperacion", conn, tx)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@Canal", SqlDbType.VarChar, 10).Value = canal
                cmd.Parameters.Add("@Monto", SqlDbType.Decimal).Value = monto
                cmd.ExecuteNonQuery()
            End Using
            tx.Commit()
        Catch ex As Exception
            tx.Rollback()
            Throw
        End Try
    End Using
End Using
```

---

### 6. Agregar un nuevo canal en arquitectura multicanal
**Debes agregar un canal nuevo a un sitio que ya usan otros canales en produccion. ¿Como lo implementas para no afectar a los canales existentes (parametrizacion frente a condiciones en el codigo) y como lo pruebas?**

**Respuesta:**

* **Parametrizacion vs condiciones en codigo:**  
  La mejor manera es hacer que el sistema sea **parametrizable por base de datos** en vez de meter `If canal = "NUEVO" Then` o `Select Case` en el codigo. Si se usan condicionales en el codigo, cada vez que entre un canal hay que modificar archivos, volver a compilar, hacer pruebas de regresion y desplegar a produccion, con el riesgo de afectar a canales como AB o CJ que ya estan operando.  
  Con la tabla de configuracion (`CanalConfiguracion`), el codigo es generico: lee la URL del servicio, la API Key, si muestra menu o no y la pagina a donde redirigir. Para dar de alta el nuevo canal, solo se ejecuta un `INSERT` en la base de datos con sus parametros. El impacto en produccion es cero codigo modificado y cero reinicios.

* **Como probarlo:**
  1. En ambiente de pruebas (QA/UAT), se inserta el nuevo canal en la base de datos.
  2. Se prueba la URL con el parametro del canal nuevo (ej: `?canal=APP`) y se valida que consuma su endpoint correspondiente, que respete la configuracion del menu y navegue a su pantalla destino.
  3. Pruebas de regresion: se verifica que los canales existentes (`AB` y `CJ`) sigan funcionando exactamente igual.
  4. Se prueba con datos incorrectos o servicio caido para confirmar que el nuevo canal muestre el mensaje de error controlado y no rompa la aplicacion.

---

### 7. Manejo centralizado de errores y registro tecnico
**¿Como manejas los errores en una aplicacion Web Forms para mostrar mensajes controlados, registrar el detalle y no exponer informacion tecnica al usuario?**

**Respuesta:**

Se maneja por capas para que el usuario nunca vea pantallas amarillas de error con stack traces ni nombres de tablas:

1. **Try/Catch en operaciones criticas:** En los llamados al servicio REST o base de datos, se capturan las excepciones (`ApiException`, `SqlException`).
2. **Mensaje amigable con Correlation ID:** Al usuario se le muestra un mensaje claro (ej: *"El servicio no esta disponible temporalmente, intente mas tarde"*) junto con un codigo de seguimiento o ticket (un GUID corto generado al momento).
3. **Registro en Log:** En el servidor se escribe el detalle tecnico completo (fecha, canal, mensaje de excepcion, inner exception y stack trace) asociado a ese mismo codigo de seguimiento. Asi soporte tecnico puede buscar exactamente que paso sin que el cliente vea detalles internos de la infraestructura.
4. **Proteccion de archivos de log:** Los logs se guardan en la carpeta `App_Data/logs`, ya que por seguridad IIS no permite descargar nada que este dentro de `App_Data`.
5. **Configuracion en Web.config (`customErrors`):** Para cualquier error no previsto, se activa `customErrors mode="RemoteOnly" defaultRedirect="~/Error.aspx"` para redirigir a una pagina de error generica.

---

### 8. Seguridad integral en Web Forms
**¿Que medidas de seguridad aplicas en Web Forms contra XSS, inyeccion SQL y manipulacion del ViewState, y que datos nunca guardarias en sesion?**

**Respuesta:**

* **Contra Inyeccion SQL:**  
  Uso de Stored Procedures y comandos con parametros tipados en ADO.NET. Nunca concatenar strings en queries. Ademas, que el usuario de base de datos de la app solo tenga permisos de ejecucion y lectura/escritura necesarios, nunca permisos de administrador del servidor (`sa`).

* **Contra XSS (Cross-Site Scripting):**  
  Tener activa la validacion de requests en el `web.config`. Al momento de mostrar datos que vienen del usuario o de servicios externos en la pantalla, usar siempre `<%:` (que codifica HTML automaticamente) o llamar a `Server.HtmlEncode()`. Tambien agregar cabeceras de respuesta como `X-Content-Type-Options: nosniff` y `X-Frame-Options`.

* **Contra Manipulacion del ViewState:**  
  Asegurarse de tener `enableViewStateMac="true"` en la configuracion para que ASP.NET valide la firma criptografica del ViewState con un hash. Si alguien altera un valor en el cliente, el servidor rechaza la peticion. Tambien se puede activar el cifrado del ViewState y definir claves fijas en la seccion `<machineKey>`.

* **Datos que NUNCA guardaria en Sesion:**
  1. Contraseñas de usuarios en texto plano o llaves maestras de cifrado.
  2. Datos completos de tarjetas de credito (numero de tarjeta, fecha y menos el CVV), ya que las normativas de seguridad (como PCI-DSS) lo prohiben estrictamente.
  3. Objetos gigantescos o tablas con miles de registros que saturen la memoria RAM del servidor o que no se puedan serializar si se usa sesion en SQL o Redis.
