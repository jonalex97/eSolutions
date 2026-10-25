Imports System
Imports System.Net
Imports System.Web
Imports System.Web.Routing
Imports eSolutions.Web.Common

Public Class Global_asax
        Inherits System.Web.HttpApplication

        Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
            ' Protocolos tls para llamadas http seguras
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls13

            ' Aumentar limite de conexiones por host
            ServicePointManager.DefaultConnectionLimit = 50

            Logger.LogInfo("Aplicación eSolutions Pagos Multicanal iniciada correctamente.")
        End Sub

        Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
            ' Redirección interna transparente para que /empresas?canal=... sea atendido por SimuladorEmpresas.ashx
            Dim ruta As String = Request.AppRelativeCurrentExecutionFilePath
            If ruta.Equals("~/empresas", StringComparison.OrdinalIgnoreCase) Then
                Context.RewritePath("~/SimuladorEmpresas.ashx", Request.PathInfo, Request.QueryString.ToString())
            End If
        End Sub

        Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
            Dim ex As Exception = Server.GetLastError()
            Dim incidentId As String = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()

            Logger.LogError($"Error no controlado a nivel de aplicación (Incidente: {incidentId})", ex, incidentId)

            ' En producción se redirigiría a Error.aspx?incident=... sin limpiar el error si IIS gestiona customErrors
        End Sub

        Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
            ' Inicialización de sesión si se requiere
        End Sub

        Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
            ' Limpieza de sesión
        End Sub
    End Class
