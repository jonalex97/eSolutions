Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Web
Imports System.Web.Script.Serialization
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models

''' <summary>
''' Handler para simular el servicio externo de empresas.
''' </summary>
Public Class SimuladorEmpresas
    Inherits Object
    Implements IHttpHandler

        Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
            context.Response.ContentType = "application/json; charset=utf-8"

            ' Simular retardo si viene en query (?delay=3000)
            Dim paramDelay As String = context.Request.QueryString("delay")
            If Not String.IsNullOrEmpty(paramDelay) Then
                Dim delayMs As Integer
                If Integer.TryParse(paramDelay, delayMs) AndAlso delayMs > 0 Then
                    Thread.Sleep(delayMs)
                End If
            End If

            ' 2. Simulación forzada de error si se solicita en query string (ej: ?error=1)
            Dim paramError As String = context.Request.QueryString("error")
            If Not String.IsNullOrEmpty(paramError) AndAlso paramError = "1" Then
                context.Response.StatusCode = 500
                context.Response.Write("{""error"": ""Error 500 simulado en el servicio de empresas""}")
                Return
            End If

            ' Validar que venga la cabecera X-Api-Key
            Dim apiKeyEnviada As String = context.Request.Headers("X-Api-Key")
            If String.IsNullOrWhiteSpace(apiKeyEnviada) Then
                context.Response.StatusCode = 401
                context.Response.Write("{""error"": ""401 Unauthorized: Falta encabezado X-Api-Key""}")
                Return
            End If

            ' Validacion de prefijo de llave pactada (KEY-SEC-*)
            Dim canal As String = If(context.Request.QueryString("canal"), "").ToUpperInvariant().Trim()
            Dim llaveValida As Boolean = False

            If apiKeyEnviada.StartsWith("KEY-SEC-", StringComparison.OrdinalIgnoreCase) Then
                llaveValida = True
            End If

            If Not llaveValida Then
                context.Response.StatusCode = 401
                context.Response.Write("{""error"": ""401 Unauthorized: La llave API provista es inválida para este servicio.""}")
                Return
            End If

            ' 4. Generación de catálogo de empresas según el canal
            Dim empresas As List(Of EmpresaModel) = ObtenerCatalogoEmpresas(canal)

            context.Response.StatusCode = 200
            Dim serializer As New JavaScriptSerializer()
            context.Response.Write(serializer.Serialize(empresas))
        End Sub

        Private Function ObtenerCatalogoEmpresas(canal As String) As List(Of EmpresaModel)
            Dim lista As New List(Of EmpresaModel)()

            Select Case canal
                Case "AB"
                    ' Empresas para canal Autoservicio Bancario
                    lista.Add(New EmpresaModel(101, "Agua Potable y Alcantarillado", "https://cdn-icons-png.flaticon.com/512/3105/3105807.png"))
                    lista.Add(New EmpresaModel(102, "Compañía Nacional de Electricidad", "https://cdn-icons-png.flaticon.com/512/2807/2807572.png"))
                    lista.Add(New EmpresaModel(103, "Telecomunicaciones e Internet Hogar", "https://cdn-icons-png.flaticon.com/512/2991/2991148.png"))
                    lista.Add(New EmpresaModel(104, "Tributos Municipales y Tasas", "https://cdn-icons-png.flaticon.com/512/2830/2830284.png"))

                Case "CJ"
                    ' Empresas para canal Cajas y Ventanilla
                    lista.Add(New EmpresaModel(201, "Electricidad y Gas Regional", "https://cdn-icons-png.flaticon.com/512/2807/2807572.png"))
                    lista.Add(New EmpresaModel(202, "Distribuidora de Gas Domiciliario", "https://cdn-icons-png.flaticon.com/512/1533/1533913.png"))
                    lista.Add(New EmpresaModel(203, "Seguros de Salud y Vida", "https://cdn-icons-png.flaticon.com/512/2966/2966327.png"))
                    lista.Add(New EmpresaModel(204, "Telefonía Móvil Corporativa", "https://cdn-icons-png.flaticon.com/512/4213/4213179.png"))
                    lista.Add(New EmpresaModel(205, "Recolección y Gestión Ambiental", "https://cdn-icons-png.flaticon.com/512/3299/3299942.png"))

                Case Else
                    ' Catálogo estándar para cualquier nuevo canal agregado por base de datos
                    lista.Add(New EmpresaModel(301, "Servicio de Agua Metropolitana", "https://cdn-icons-png.flaticon.com/512/3105/3105807.png"))
                    lista.Add(New EmpresaModel(302, "Energía y Alumbrado Público", "https://cdn-icons-png.flaticon.com/512/2807/2807572.png"))
                    lista.Add(New EmpresaModel(303, "Internet y Televisión por Fibra", "https://cdn-icons-png.flaticon.com/512/2991/2991148.png"))
            End Select

            Return lista
        End Function

        Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
            Get
                Return True
            End Get
        End Property
    End Class
